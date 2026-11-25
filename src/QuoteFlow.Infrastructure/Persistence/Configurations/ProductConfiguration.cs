using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuoteFlow.Domain.Entities;

namespace QuoteFlow.Infrastructure.Persistence.Configurations;

/// <summary>Tabla <c>products</c> (sección 9.3).</summary>
internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products", table =>
        {
            table.HasCheckConstraint("ck_products_name_not_empty", "length(trim(name)) > 0");
            table.HasCheckConstraint("ck_products_price_not_negative", "price >= 0");
        });

        builder.ConfigureAggregateRoot();

        builder.Property(p => p.Name)
            .HasMaxLength(Product.NameMaxLength)
            .IsRequired();

        builder.Property(p => p.Description).HasMaxLength(Product.DescriptionMaxLength);
        builder.Property(p => p.Price).IsMoney().IsRequired();

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.CategoryId);

        // Nombre único sin distinguir mayúsculas (RN-30): índice sobre lower(name), creado con SQL
        // explícito en la migración (ver ManualSchemaObjects), porque EF Core no declara índices de expresión.
    }
}
