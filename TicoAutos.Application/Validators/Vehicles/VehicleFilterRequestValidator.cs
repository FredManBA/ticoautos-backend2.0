using FluentValidation;
using TicoAutos.Application.DTOs.Vehicles;

namespace TicoAutos.Application.Validators.Vehicles;

public class VehicleFilterRequestValidator : AbstractValidator<VehicleFilterRequest>
{
    public VehicleFilterRequestValidator()
    {
        RuleFor(x => x.Brand).MaximumLength(50);
        RuleFor(x => x.Model).MaximumLength(50);
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);

        RuleFor(x => x.MinYear)
            .InclusiveBetween(1900, DateTime.Now.Year + 1)
            .When(x => x.MinYear.HasValue);

        RuleFor(x => x.MaxYear)
            .InclusiveBetween(1900, DateTime.Now.Year + 1)
            .When(x => x.MaxYear.HasValue);

        RuleFor(x => x)
            .Must(x => !x.MinYear.HasValue || !x.MaxYear.HasValue || x.MinYear <= x.MaxYear)
            .WithMessage("El año mínimo no puede ser mayor que el año máximo.");

        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinPrice.HasValue);

        RuleFor(x => x.MaxPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaxPrice.HasValue);

        RuleFor(x => x)
            .Must(x => !x.MinPrice.HasValue || !x.MaxPrice.HasValue || x.MinPrice <= x.MaxPrice)
            .WithMessage("El precio mínimo no puede ser mayor que el precio máximo.");
    }
}
