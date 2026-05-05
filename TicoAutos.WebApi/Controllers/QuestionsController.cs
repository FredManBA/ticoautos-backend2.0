using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicoAutos.Application.DTOs.Questions;
using TicoAutos.Domain.Entities;
using TicoAutos.Domain.Interfaces;

namespace TicoAutos.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuestionsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IContactModerationService _contactModerationService;

    public QuestionsController(IUnitOfWork unitOfWork, IContactModerationService contactModerationService)
    {
        _unitOfWork = unitOfWork;
        _contactModerationService = contactModerationService;
    }

    /// <summary>
    /// Authenticated buyer asks a question about a vehicle.
    /// POST /api/questions/{vehicleId}
    /// </summary>
    [HttpPost("{vehicleId}")]
    [Authorize]
    public async Task<IActionResult> Ask(int vehicleId, [FromBody] AskQuestionRequest request)
    {
        var userIdClaim = User.FindFirst("id")?.Value;
        if (userIdClaim is null || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized(new { message = "Token inválido." });

        var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(vehicleId);
        if (vehicle is null) return NotFound(new { message = "Vehículo no encontrado." });

        if (vehicle.OwnerId == userId)
            return BadRequest(new { message = "No puede realizar preguntas en su propio vehículo." });

        if (vehicle.IsSold)
            return Conflict(new { message = "No se pueden realizar preguntas en vehículos vendidos." });

        var moderation = await _contactModerationService.ValidateAsync(request.Content, HttpContext.RequestAborted);
        if (!moderation.IsAllowed)
            return BadRequest(new { message = moderation.Message, detectedTypes = moderation.DetectedTypes });

        var question = new Question
        {
            Content = request.Content,
            VehicleId = vehicleId,
            AskerId = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Questions.AddAsync(question);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { question.Id, question.Content, question.CreatedAt });
    }

    /// <summary>
    /// Vehicle owner answers a question.
    /// POST /api/questions/{questionId}/answer
    /// </summary>
    [HttpPost("{questionId}/answer")]
    [Authorize]
    public async Task<IActionResult> Answer(int questionId, [FromBody] AnswerQuestionRequest request)
    {
        var userIdClaim = User.FindFirst("id")?.Value;
        if (userIdClaim is null || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized(new { message = "Token inválido." });

        var question = await _unitOfWork.Questions.GetByIdWithVehicleAsync(questionId);
        if (question is null) return NotFound(new { message = "Pregunta no encontrada." });

        if (question.Vehicle.OwnerId != userId)
            return Forbid();

        if (question.Answer is not null)
            return Conflict(new { message = "Esta pregunta ya tiene respuesta." });

        var moderation = await _contactModerationService.ValidateAsync(request.Content, HttpContext.RequestAborted);
        if (!moderation.IsAllowed)
            return BadRequest(new { message = moderation.Message, detectedTypes = moderation.DetectedTypes });

        var answer = new Answer
        {
            Content = request.Content,
            QuestionId = questionId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Answers.AddAsync(answer);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { answer.Id, answer.Content, answer.CreatedAt });
    }

    /// <summary>
    /// Get questions asked by the authenticated user.
    /// GET /api/questions/my
    /// </summary>
    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMyQuestions()
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return Unauthorized(new { message = "Token inválido." });

        var questions = await _unitOfWork.Questions.GetByAskerIdAsync(userId);
        return Ok(MapQuestionResponses(questions));
    }

    /// <summary>
    /// Get all questions associated with vehicles owned by the authenticated user.
    /// GET /api/questions/inbox
    /// </summary>
    [HttpGet("inbox")]
    [Authorize]
    public async Task<IActionResult> GetInbox()
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return Unauthorized(new { message = "Token inválido." });

        var questions = await _unitOfWork.Questions.GetByOwnerIdAsync(userId);
        return Ok(MapQuestionResponses(questions));
    }

    /// <summary>
    /// Get the question history for one vehicle owned by the authenticated user.
    /// GET /api/questions/vehicle/{vehicleId}/owner
    /// </summary>
    [HttpGet("vehicle/{vehicleId}/owner")]
    [Authorize]
    public async Task<IActionResult> GetOwnerVehicleHistory(int vehicleId)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return Unauthorized(new { message = "Token inválido." });

        var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(vehicleId);
        if (vehicle is null) return NotFound(new { message = "Vehículo no encontrado." });

        if (vehicle.OwnerId != userId)
            return Forbid();

        var questions = await _unitOfWork.Questions.GetByVehicleIdForOwnerAsync(vehicleId, userId);
        return Ok(MapQuestionResponses(questions));
    }

    /// <summary>
    /// Get all questions for a vehicle with their answers.
    /// GET /api/questions/vehicle/{vehicleId}
    /// </summary>
    [HttpGet("vehicle/{vehicleId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByVehicle(int vehicleId)
    {
        var questions = await _unitOfWork.Questions.GetByVehicleIdAsync(vehicleId);

        return Ok(MapQuestionResponses(questions));
    }

    private static IEnumerable<QuestionResponseDto> MapQuestionResponses(IEnumerable<Question> questions)
    {
        return questions.Select(q => new QuestionResponseDto
        {
            Id = q.Id,
            Content = q.Content,
            CreatedAt = q.CreatedAt,
            VehicleId = q.VehicleId,
            AskerId = q.AskerId,
            AskerName = q.Asker?.Name ?? "Usuario",
            Answer = q.Answer == null ? null : new AnswerResponseDto
            {
                Id = q.Answer.Id,
                Content = q.Answer.Content,
                CreatedAt = q.Answer.CreatedAt
            }
        });
    }

    private bool TryGetAuthenticatedUserId(out int userId)
    {
        var userIdClaim = User.FindFirst("id")?.Value;
        return int.TryParse(userIdClaim, out userId);
    }
}
