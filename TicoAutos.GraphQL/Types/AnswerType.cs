namespace TicoAutos.GraphQL.Types;

/// <summary>
/// GraphQL type definition for Answer.
/// Maps the Answer domain entity to a GraphQL type with its exposed fields.
/// </summary>
public class AnswerType
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}