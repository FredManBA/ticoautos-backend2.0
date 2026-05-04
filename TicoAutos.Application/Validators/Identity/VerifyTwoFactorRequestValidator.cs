using FluentValidation;
using TicoAutos.Application.DTOs;

namespace TicoAutos.Application.Validators.Identity;

public class VerifyTwoFactorRequestValidator : AbstractValidator<VerifyTwoFactorRequest>
{
    public VerifyTwoFactorRequestValidator()
    {
        RuleFor(x => x.TemporaryToken)
            .NotEmpty().WithMessage("El token temporal de doble factor es requerido.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("El código de verificación es requerido.")
            .Matches(@"^\d{6}$").WithMessage("El código debe tener 6 dígitos.");
    }
}
