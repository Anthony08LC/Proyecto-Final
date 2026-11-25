using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuoteFlow.Domain.Common;
using QuoteFlow.Domain.Entities;
using QuoteFlow.Domain.ValueObjects;

namespace QuoteFlow.Infrastructure.Persistence.Configurations;

internal static class EntityTypeBuilderExtensions
{
    /// <summary>Nombre de la propiedad sombra mapeada a la columna de sistema <c>xmin</c>.</summary>
    public const string RowVersion = "RowVersion";

    /// <summary>
    /// Configuración común de las raíces de agregado: identificador UUID v7 generado por el dominio,
    /// campos de auditoría (sección 9.2) con llave foránea a <c>users</c> y control de concurrencia
    /// optimista mediante <c>xmin</c> (RNF-DAT-04).
    /// </summary>
    public static void ConfigureAggregateRoot<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : AuditableEntity
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.CreatedBy).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();
        builder.Property(e => e.UpdatedBy).IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.UpdatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property<uint>(RowVersion)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .IsRowVersion();
    }

    public static PropertyBuilder<Money> IsMoney(this PropertyBuilder<Money> property) =>
        property
            .HasConversion(ValueObjectConverters.Money)
            .HasPrecision(ColumnTypes.MoneyPrecision, ColumnTypes.Scale);

    public static ComplexTypePropertyBuilder<Money> IsMoney(this ComplexTypePropertyBuilder<Money> property) =>
        property
            .HasConversion(ValueObjectConverters.Money)
            .HasPrecision(ColumnTypes.MoneyPrecision, ColumnTypes.Scale)
            .IsRequired();

    public static PropertyBuilder<Percentage> IsPercentage(this PropertyBuilder<Percentage> property) =>
        property
            .HasConversion(ValueObjectConverters.Percentage)
            .HasPrecision(ColumnTypes.PercentagePrecision, ColumnTypes.Scale);

    public static ComplexTypePropertyBuilder<Percentage> IsPercentage(
        this ComplexTypePropertyBuilder<Percentage> property) =>
        property
            .HasConversion(ValueObjectConverters.Percentage)
            .HasPrecision(ColumnTypes.PercentagePrecision, ColumnTypes.Scale)
            .IsRequired();

    public static PropertyBuilder<Email?> IsEmail(this PropertyBuilder<Email?> property) =>
        property
            .HasConversion(ValueObjectConverters.Email!)
            .HasMaxLength(Email.MaxLength);

    public static PropertyBuilder<Phone?> IsPhone(this PropertyBuilder<Phone?> property) =>
        property
            .HasConversion(ValueObjectConverters.Phone!)
            .HasMaxLength(Phone.MaxLength);
}
