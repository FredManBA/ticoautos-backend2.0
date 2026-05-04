using FluentValidation;
using TicoAutos.Application.DTOs.Questions;

namespace TicoAutos.Application.Validators.Questions;

public class AskQuestionRequestValidator : AbstractValidator<AskQuestionRequest>
{
    public AskQuestionRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("La pregunta es requerida.")
            .MinimumLength(5).WithMessage("La pregunta debe tener al menos 5 caracteres.")
            .MaximumLength(500).WithMessage("La pregunta no puede superar 500 caracteres.")
            .Must(ContactInfoValidator.DoesNotContainContactInfo)
            .WithMessage(ContactInfoValidator.Message);
    }
}
