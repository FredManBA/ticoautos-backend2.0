using FluentValidation;
using TicoAutos.Application.DTOs.Questions;

namespace TicoAutos.Application.Validators.Questions;

public class AskQuestionRequestValidator : AbstractValidator<AskQuestionRequest>
{
    public AskQuestionRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("La pregunta es requerida.")
            .MaximumLength(500).WithMessage("La pregunta no puede superar 500 caracteres.");
    }
}
