using FluentValidation;
using TicoAutos.Application.DTOs.Questions;

namespace TicoAutos.Application.Validators.Questions;

public class AnswerQuestionRequestValidator : AbstractValidator<AnswerQuestionRequest>
{
    public AnswerQuestionRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("La respuesta es requerida.")
            .MinimumLength(3).WithMessage("La respuesta debe tener al menos 3 caracteres.")
            .MaximumLength(500).WithMessage("La respuesta no puede superar 500 caracteres.")
            .Must(ContactInfoValidator.DoesNotContainContactInfo)
            .WithMessage(ContactInfoValidator.Message);
    }
}
