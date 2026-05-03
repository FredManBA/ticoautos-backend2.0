using Microsoft.EntityFrameworkCore;
using TicoAutos.Domain.Interfaces;
using TicoAutos.GraphQL.Types;

namespace TicoAutos.GraphQL.Queries;

/// <summary>
/// GraphQL queries for Question entity.
/// Uses IUnitOfWork following Clean Architecture — no direct DbContext access.
/// </summary>
[QueryType]
public class QuestionQuery
{
    /// <summary>
    /// Returns all questions for a specific vehicle including their answers.
    /// </summary>
    public async Task<IReadOnlyList<QuestionType>> GetQuestionsByVehicle(
        [Service] IUnitOfWork unitOfWork,
        int vehicleId)
    {
        var questions = await unitOfWork.Questions
            .GetByVehicleIdAsync(vehicleId);

        return questions.Select(q => new QuestionType
        {
            Id = q.Id,
            Content = q.Content,
            CreatedAt = q.CreatedAt,
            VehicleId = q.VehicleId,
            AskerId = q.AskerId,
            AskerName = q.Asker?.Name ?? "Usuario",
            Answer = q.Answer is null ? null : new AnswerType
            {
                Id = q.Answer.Id,
                Content = q.Answer.Content,
                CreatedAt = q.Answer.CreatedAt
            }
        }).ToList();
    }

    /// <summary>
    /// Returns questions asked by the authenticated user.
    /// Requires a valid JWT token.
    /// </summary>
    [HotChocolate.Authorization.Authorize]
    public async Task<IReadOnlyList<QuestionType>> GetMyQuestions(
        [Service] IUnitOfWork unitOfWork,
        [GlobalState("userId")] int userId)
    {
        var questions = await unitOfWork.Questions
            .GetByAskerIdAsync(userId);

        return questions.Select(q => new QuestionType
        {
            Id = q.Id,
            Content = q.Content,
            CreatedAt = q.CreatedAt,
            VehicleId = q.VehicleId,
            AskerId = q.AskerId,
            AskerName = q.Asker?.Name ?? "Usuario",
            Answer = q.Answer is null ? null : new AnswerType
            {
                Id = q.Answer.Id,
                Content = q.Answer.Content,
                CreatedAt = q.Answer.CreatedAt
            }