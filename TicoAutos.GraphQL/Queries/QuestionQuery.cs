using HotChocolate;
using TicoAutos.Domain.Interfaces;
using TicoAutos.GraphQL.Types;

namespace TicoAutos.GraphQL.Queries;

[QueryType]
public class QuestionQuery
{
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

    public async Task<IReadOnlyList<QuestionType>> GetMyQuestions(
        [Service] IUnitOfWork unitOfWork,
        [GlobalState("userId")] int userId)
    {
        if (userId == 0)
            throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage("Debe iniciar sesión para ver sus preguntas.")
                    .SetCode("UNAUTHENTICATED")
                    .Build());

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
        }).ToList();
    }
}
