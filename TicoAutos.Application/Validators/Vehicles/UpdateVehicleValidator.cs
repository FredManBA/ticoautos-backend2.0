using FluentValidation;
using TicoAutos.Application.DTOs.Vehicles;

namespace TicoAutos.Application.Validators.Vehicles;

public class UpdateVehicleValidator : AbstractValidator<UpdateVehicleRequest>
{
    public UpdateVehicleValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Brand).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Model).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Year).InclusiveBetween(1900, DateTime.Now.Year + 1);
        RuleFor(x => x.Price).GreaterThan(0).WithMessage("El precio debe ser mayor a cero.");
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.ImageUrl).MaximumLength(500);
    }
}
