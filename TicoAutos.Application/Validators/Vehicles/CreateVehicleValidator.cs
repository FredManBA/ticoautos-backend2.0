using FluentValidation;
using TicoAutos.Application.DTOs.Vehicles;

namespace TicoAutos.Application.Validators.Vehicles;

public class CreateVehicleValidator : AbstractValidator<CreateVehicleRequest>
{
    /// <summary>
    /// Implements validation rules for the CreateVehicleRequest DTO, ensuring that the data provided for creating a new vehicle adheres to defined constraints and business rules,
    /// </summary>
    public CreateVehicleValidator()
    {
        RuleFor(x => x.Brand).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Model).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Year).InclusiveBetween(1900, DateTime.Now.Year + 1);
        RuleFor(x => x.Price).GreaterThan(0).WithMessage("El precio debe ser mayor a cero.");
        RuleFor(x => x.Description).MaximumLength(500);
    }
}