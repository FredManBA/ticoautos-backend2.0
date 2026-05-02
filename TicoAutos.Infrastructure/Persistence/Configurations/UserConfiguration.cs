using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicoAutos.Domain.Entities;

namespace TicoAutos.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(u => u.Email)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(u => u.Cedula)
            .HasMaxLength(9);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.HasIndex(u => u.Cedula)
            .IsUnique()
            .HasFilter("[Cedula] IS NOT NULL");
    }
}
