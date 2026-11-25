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

        // RN-30 exige unicidad sin distinguir mayúsculas: la aplicación la valida con lower(name)
        // y la migración debe reemplazar este índice por uno único sobre lower(name).
        builder.HasIndex(p => p.Name)
            .IsUnique()
            .HasDatabaseName("uq_products_name");
    }
}
