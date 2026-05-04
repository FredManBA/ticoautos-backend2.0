using FluentValidation;
using TicoAutos.Application.DTOs;

namespace TicoAutos.Application.Validators.Identity;

public class ResendVerificationRequestValidator : AbstractValidator<ResendVerificationRequest>
{
    public ResendVerificationRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo es requerido.")
            .EmailAddress().WithMessage("El formato del correo no es válido.");
    }
}
