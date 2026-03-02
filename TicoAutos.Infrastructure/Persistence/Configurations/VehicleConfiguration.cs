using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicoAutos.Domain.Entities;


namespace TicoAutos.Infrastructure.Persistence.Configurations;



public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{

    /// <summary>
    /// Metohd to configure the Vehicle entity using the Fluent API. This method is called by the DbContext when building the model.
    /// </summary>
    /// <param name="builder"></param>
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("Vehicles");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Brand)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(v => v.Model)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(v => v.Price)
            .HasPrecision(18, 2);
    }
}