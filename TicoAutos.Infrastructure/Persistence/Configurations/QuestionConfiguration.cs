using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicoAutos.Domain.Entities;

namespace TicoAutos.Infrastructure.Persistence.Configurations;

public class QuestionConfiguration : IEntityTypeConfiguration<Question>
{

    /// <summary>
    /// Method to configure the Question entity's relationships and constraints using the Entity Framework Core Fluent API.
    /// </summary>
    /// <param name="builder"></param>
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.HasOne(q => q.Asker)
            .WithMany()
            .HasForeignKey(q => q.AskerId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}