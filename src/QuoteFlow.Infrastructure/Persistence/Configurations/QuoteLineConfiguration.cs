using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuoteFlow.Domain.Entities;

namespace QuoteFlow.Infrastructure.Persistence.Configurations;

/// <summary>
/// Tabla <c>quote_lines</c> (sección 9.3). Las líneas pertenecen al agregado <see cref="Quote"/>:
/// la llave foránea a la cotización es una propiedad sombra.
/// </summary>
internal sealed class QuoteLineConfiguration : IEntityTypeConfiguration<QuoteLine>
{
    public const string QuoteIdProperty = "QuoteId";
    private const string LineTotalProperty = "LineTotal";

    public void Configure(EntityTypeBuilder<QuoteLine> builder)
    {
        builder.ToTable("quote_lines", table =>
        {
            table.HasCheckConstraint("ck_quote_lines_line_number_positive", "line_number > 0");
            table.HasCheckConstraint("ck_quote_lines_quantity_positive", "quantity > 0");
            table.HasCheckConstraint("ck_quote_lines_unit_price_not_negative", "unit_price >= 0");
        });

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).ValueGeneratedNever();

        builder.Property<Guid>(QuoteIdProperty).IsRequired();

        builder.Property(l => l.LineNumber)
            .HasColumnType("smallint")
            .IsRequired();

        builder.Property(l => l.ProductName)
            .HasMaxLength(Product.NameMaxLength)
            .IsRequired();

        builder.Property(l => l.ProductDescription).HasMaxLength(Product.DescriptionMaxLength);

        builder.Property(l => l.CategoryName)
            .HasMaxLength(Category.NameMaxLength)
            .IsRequired();

        builder.Property(l => l.UnitPrice).IsMoney().IsRequired();

        builder.Property(l => l.Quantity)
            .HasPrecision(ColumnTypes.QuantityPrecision, ColumnTypes.Scale)
            .IsRequired();

        builder.Property(l => l.DiscountPercentage)
            .IsPercentage()
            .IsRequired();

        // Total de la línea calculado por la base de datos con el mismo redondeo que el motor
        // (RN-17: round de PostgreSQL sobre numeric aleja los valores medios de cero).
        builder.Property<decimal>(LineTotalProperty)
            .HasPrecision(ColumnTypes.MoneyPrecision, ColumnTypes.Scale)
            .HasComputedColumnSql(
                "round(unit_price * quantity, 2) - round(round(unit_price * quantity, 2) * discount_percentage / 100, 2)",
                stored: true);

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(l => l.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Un producto no se repite en la misma cotización (RN-28).
        builder.HasIndex(QuoteIdProperty, nameof(QuoteLine.ProductId))
            .IsUnique()
            .HasDatabaseName("uq_quote_lines_quote_product");

        // (quote_id, line_number) es único con verificación diferida al final de la transacción, porque
        // renumerar al quitar una línea viola la unicidad temporalmente. EF Core no declara restricciones
        // DEFERRABLE: se crea con SQL explícito en la migración (ver ManualSchemaObjects).

        builder.HasIndex(l => l.ProductId);
    }
}
