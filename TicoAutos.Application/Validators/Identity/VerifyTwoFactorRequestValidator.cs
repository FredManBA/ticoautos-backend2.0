using FluentValidation;
using TicoAutos.Application.DTOs;

namespace TicoAutos.Application.Validators.Identity;

public class VerifyTwoFactorRequestValidator : AbstractValidator<VerifyTwoFactorRequest>
{
    public VerifyTwoFactorRequestValidator()
    {
        RuleFor(x => x.TwoFactorToken)
            .NotEmpty().WithMessage("El token de doble factor es requerido.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("El codigo de verificacion es requerido.")
            .Matches(@"^\d{6}$").WithMessage("El codigo debe tener 6 digitos.");
    }
}
