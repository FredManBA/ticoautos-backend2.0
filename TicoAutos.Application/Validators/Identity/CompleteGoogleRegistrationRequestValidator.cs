using FluentValidation;
using TicoAutos.Application.DTOs;

namespace TicoAutos.Application.Validators.Identity;

public class CompleteGoogleRegistrationRequestValidator : AbstractValidator<CompleteGoogleRegistrationRequest>
{
    public CompleteGoogleRegistrationRequestValidator()
    {
        RuleFor(x => x.RegistrationToken)
            .NotEmpty().WithMessage("El token de registro es requerido.");

        RuleFor(x => x.Cedula)
            .NotEmpty().WithMessage("La cédula es requerida.")
            .Matches(@"^\d{9}$").WithMessage("La cédula debe tener 9 dígitos, sin guiones ni espacios.");
    }
}
