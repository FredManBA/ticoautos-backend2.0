namespace TicoAutos.WebApi.GraphQL;

public record VehicleGraphQlDto(
    int Id,
    string Brand,
    string Model,
    int Year,
    decimal Price,
    string Description,
    string ImageUrl,
    bool IsSold,
    DateTime CreatedAt,
    int OwnerId,
    string OwnerName,
    int UnansweredQuestions);

public record QuestionGraphQlDto(
    int Id,
    string Content,
    DateTime CreatedAt,
    int VehicleId,
    int AskerId,
    string AskerName,
    AnswerGraphQlDto? Answer);

public record AnswerGraphQlDto(
    int Id,
    string Content,
    DateTime CreatedAt);
