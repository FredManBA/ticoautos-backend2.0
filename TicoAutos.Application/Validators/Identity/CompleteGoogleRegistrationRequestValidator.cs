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
            .NotEmpty().WithMessage("La cedula es requerida.")
            .Matches(@"^\d{9}$").WithMessage("La cedula debe tener 9 digitos, sin guiones ni espacios.");
    }
}
