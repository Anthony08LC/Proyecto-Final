using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuoteFlow.Infrastructure.Persistence.Records;

namespace QuoteFlow.Infrastructure.Persistence.Configurations;

/// <summary>Tabla <c>quote_number_sequences</c> (sección 9.3).</summary>
internal sealed class QuoteNumberSequenceConfiguration : IEntityTypeConfiguration<QuoteNumberSequence>
{
    public void Configure(EntityTypeBuilder<QuoteNumberSequence> builder)
    {
        builder.ToTable("quote_number_sequences", table =>
        {
            table.HasCheckConstraint(
                "ck_quote_number_sequences_year",
                $"year BETWEEN {QuoteNumberSequence.MinYear} AND {QuoteNumberSequence.MaxYear}");
            table.HasCheckConstraint(
                "ck_quote_number_sequences_last_number",
                $"last_number BETWEEN 0 AND {QuoteNumberSequence.MaxNumber}");
        });

        builder.HasKey(s => s.Year);
        builder.Property(s => s.Year).ValueGeneratedNever();
        builder.Property(s => s.LastNumber).HasDefaultValue(0).IsRequired();
    }
}
