namespace TicoAutos.GraphQL.Types;

/// <summary>
/// GraphQL type definition for Question.
/// Maps the Question domain entity to a GraphQL type with its exposed fields.
/// </summary>
public class QuestionType
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int VehicleId { get; set; }
    public int AskerId { get; set; }
    public string AskerName { get; set; } = string.Empty;
    public AnswerType? Answer { get; set; }
}