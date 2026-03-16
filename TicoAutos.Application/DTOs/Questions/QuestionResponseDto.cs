namespace TicoAutos.Application.DTOs.Questions;

public class QuestionResponseDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int VehicleId { get; set; }
    public int AskerId { get; set; }
    public string AskerName { get; set; } = string.Empty;
    public AnswerResponseDto? Answer { get; set; }
}