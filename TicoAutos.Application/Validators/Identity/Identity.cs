// TicoAutos.Application/Validators/Identity/RegisterRequestValidator.cs
using FluentValidation;
using TicoAutos.Application.DTOs;

namespace TicoAutos.Application.Validators.Identity;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo es requerido.")
            .EmailAddress().WithMessage("El formato del correo no es válido.");

        RuleFor(x => x.Cedula)
            .NotEmpty().WithMessage("La cédula es requerida.")
            .Matches(@"^\d{9}$").WithMessage("La cédula debe tener 9 dígitos, sin guiones ni espacios.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("El telefono es requerido.")
            .Matches(@"^\+506\d{8}$").WithMessage("El telefono debe usar el formato +506########.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es requerida.")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
            .Matches("[A-Z]").WithMessage("Debe contener al menos una letra mayúscula.")
            .Matches("[0-9]").WithMessage("Debe contener al menos un número.");
    }
}
