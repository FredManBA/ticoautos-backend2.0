using TicoAutos.Domain.Entities;

public class Answer
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Relationship with Question
    public int QuestionId { get; set; }
    public Question Question { get; set; } = null!;
}