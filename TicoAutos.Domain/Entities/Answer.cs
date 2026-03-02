using TicoAutos.Domain.Entities;


/// <summary>
/// Identifies an answer provided by a seller in response to a question asked by a potential buyer about a specific vehicle, encapsulating its properties and relationships within the domain model of the TicoAutos application.
/// </summary>
public class Answer
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Relationship with Question
    public int QuestionId { get; set; }
    public Question Question { get; set; } = null!;
}