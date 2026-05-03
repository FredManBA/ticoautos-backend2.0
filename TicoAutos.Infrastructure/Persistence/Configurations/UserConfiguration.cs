using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicoAutos.Domain.Constants;
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

        builder.Property(u => u.AuthProvider)
            .HasMaxLength(40)
            .HasDefaultValue(AuthProviders.Local)
            .IsRequired();

        builder.Property(u => u.ExternalProviderId)
            .HasMaxLength(128);

        builder.Property(u => u.AccountStatus)
            .HasMaxLength(40)
            .HasDefaultValue(AccountStatuses.Active)
            .IsRequired();

        builder.Property(u => u.IsEmailVerified)
            .HasDefaultValue(true);

        builder.Property(u => u.EmailVerificationToken)
            .HasMaxLength(128);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.HasIndex(u => u.Cedula)
            .IsUnique()
            .HasFilter("[Cedula] IS NOT NULL");

        builder.HasIndex(u => u.EmailVerificationToken)
            .IsUnique()
            .HasFilter("[EmailVerificationToken] IS NOT NULL");

        builder.HasIndex(u => new { u.AuthProvider, u.ExternalProviderId })
            .IsUnique()
            .HasFilter("[ExternalProviderId] IS NOT NULL");
    }
}
