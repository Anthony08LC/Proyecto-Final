using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuoteFlow.Domain.Entities;

namespace QuoteFlow.Infrastructure.Persistence.Configurations;

/// <summary>Tabla <c>categories</c> (sección 9.3).</summary>
internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories", table =>
            table.HasCheckConstraint("ck_categories_name_not_empty", "length(trim(name)) > 0"));

        builder.ConfigureAggregateRoot();

        builder.Property(c => c.Name)
            .HasMaxLength(Category.NameMaxLength)
            .IsRequired();

        builder.Property(c => c.Description).HasMaxLength(Category.DescriptionMaxLength);

        // Nombre único sin distinguir mayúsculas (RN-30): índice sobre lower(name), creado con SQL
        // explícito en la migración (ver ManualSchemaObjects), porque EF Core no declara índices de expresión.
    }
}
