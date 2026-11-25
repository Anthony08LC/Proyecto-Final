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

        // RN-30 exige unicidad sin distinguir mayúsculas: la aplicación la valida con lower(name)
        // y la migración debe reemplazar este índice por uno único sobre lower(name).
        builder.HasIndex(c => c.Name)
            .IsUnique()
            .HasDatabaseName("uq_categories_name");
    }
}
