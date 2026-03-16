namespace TicoAutos.Application.DTOs.Questions;

public class AnswerResponseDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}