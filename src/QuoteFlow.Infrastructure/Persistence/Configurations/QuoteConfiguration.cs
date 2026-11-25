using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuoteFlow.Domain.Entities;
using QuoteFlow.Domain.Enums;

namespace QuoteFlow.Infrastructure.Persistence.Configurations;

/// <summary>
/// Tabla <c>quotes</c> (sección 9.3). El desglose (<see cref="Domain.ValueObjects.QuoteBreakdown"/>)
/// se mapea como tipo complejo en columnas de la misma tabla. Las restricciones CHECK actúan como red
/// de seguridad de la aritmética y de la coherencia con el estado (RNF-DAT-05).
/// </summary>
internal sealed class QuoteConfiguration : IEntityTypeConfiguration<Quote>
{
    public void Configure(EntityTypeBuilder<Quote> builder)
    {
        builder.ToTable("quotes", table =>
        {
            table.HasCheckConstraint(
                "ck_quotes_status",
                "status IN ('Draft', 'Issued', 'Accepted', 'Rejected', 'Expired', 'Cancelled')");
            table.HasCheckConstraint("ck_quotes_number_format", "number IS NULL OR number ~ '^COT-[0-9]{4}-[0-9]{6}$'");

            // Aritmética del desglose.
            table.HasCheckConstraint(
                "ck_quotes_amounts_not_negative",
                "subtotal >= 0 AND discount_amount >= 0 AND taxable_base >= 0 AND vat_amount >= 0 AND total >= 0");
            table.HasCheckConstraint(
                "ck_quotes_discount_rate",
                "discount_rate = tier_discount_rate + vip_discount_rate AND discount_rate <= 100");
            table.HasCheckConstraint("ck_quotes_discount_not_above_subtotal", "discount_amount <= subtotal");
            table.HasCheckConstraint("ck_quotes_taxable_base", "taxable_base = subtotal - discount_amount");
            table.HasCheckConstraint("ck_quotes_total", "total = taxable_base + vat_amount");

            // Coherencia con el estado.
            table.HasCheckConstraint(
                "ck_quotes_draft",
                "status <> 'Draft' OR (number IS NULL AND issued_at IS NULL AND valid_until IS NULL AND closed_at IS NULL)");
            table.HasCheckConstraint(
                "ck_quotes_issued_data",
                "status NOT IN ('Issued', 'Accepted', 'Rejected', 'Expired') OR "
                + "(number IS NOT NULL AND issued_at IS NOT NULL AND valid_until IS NOT NULL "
                + "AND customer_name IS NOT NULL AND customer_is_vip IS NOT NULL)");
            table.HasCheckConstraint(
                "ck_quotes_cancelled_number",
                "status <> 'Cancelled' OR ((number IS NULL) = (issued_at IS NULL))");
            table.HasCheckConstraint(
                "ck_quotes_closed_at",
                "(status IN ('Draft', 'Issued')) = (closed_at IS NULL)");
            table.HasCheckConstraint(
                "ck_quotes_cancelled_reason",
                "status <> 'Cancelled' OR issued_at IS NULL OR close_reason IS NOT NULL");
        });

        builder.ConfigureAggregateRoot();

        builder.Property(q => q.Number).HasMaxLength(15);

        builder.Property(q => q.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // Textos comerciales.
        builder.Property(q => q.Introduction).HasMaxLength(Quote.TextMaxLength);
        builder.Property(q => q.ExecutiveSummary).HasMaxLength(Quote.TextMaxLength);
        builder.Property(q => q.Notes).HasMaxLength(Quote.TextMaxLength);
        builder.Property(q => q.CommercialTerms).HasMaxLength(Quote.TextMaxLength);

        // Datos del cliente congelados al emitir (RN-12).
        builder.Property(q => q.CustomerName).HasMaxLength(Customer.NameMaxLength);
        builder.Property(q => q.CustomerCompany).HasMaxLength(Customer.CompanyMaxLength);
        builder.Property(q => q.CustomerEmail).IsEmail();
        builder.Property(q => q.CustomerPhone).IsPhone();

        // Desglose financiero: estimado en borradores, congelado al emitir.
        builder.ComplexProperty(q => q.Breakdown, breakdown =>
        {
            breakdown.Property(b => b.Subtotal).IsMoney().HasColumnName("subtotal");
            breakdown.Property(b => b.TierDiscountRate).IsPercentage().HasColumnName("tier_discount_rate");
            breakdown.Property(b => b.VipDiscountRate).IsPercentage().HasColumnName("vip_discount_rate");
            breakdown.Property(b => b.DiscountRate).IsPercentage().HasColumnName("discount_rate");
            breakdown.Property(b => b.DiscountAmount).IsMoney().HasColumnName("discount_amount");
            breakdown.Property(b => b.TaxableBase).IsMoney().HasColumnName("taxable_base");
            breakdown.Property(b => b.VatRate).IsPercentage().HasColumnName("vat_rate");
            breakdown.Property(b => b.VatAmount).IsMoney().HasColumnName("vat_amount");
            breakdown.Property(b => b.Total).IsMoney().HasColumnName("total");
        });

        // Ciclo de vida.
        builder.Property(q => q.CloseReason).HasMaxLength(Quote.CloseReasonMaxLength);

        // Relaciones (solo por identificador entre agregados).
        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(q => q.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(q => q.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Quote>()
            .WithMany()
            .HasForeignKey(q => q.SourceQuoteId)
            .OnDelete(DeleteBehavior.Restrict);

        // Las líneas forman parte del agregado: se cargan siempre y se borran en cascada.
        builder.HasMany(q => q.Lines)
            .WithOne()
            .HasForeignKey(QuoteLineConfiguration.QuoteIdProperty)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(q => q.Lines)
            .HasField("_lines")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();

        // Índices (sección 9.5).
        builder.HasIndex(q => q.Number)
            .IsUnique()
            .HasDatabaseName("uq_quotes_number");
        builder.HasIndex(q => q.CustomerId);
        builder.HasIndex(q => q.OwnerId);
        builder.HasIndex(q => q.ValidUntil)
            .HasFilter($"status = '{nameof(QuoteStatus.Issued)}'")
            .HasDatabaseName("ix_quotes_valid_until_issued");
    }
}
