using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuoteFlow.Domain.Entities;
using QuoteFlow.Domain.ValueObjects;

namespace QuoteFlow.Infrastructure.Persistence.Configurations;

/// <summary>
/// Tabla <c>system_settings</c>: registro único (sección 9.3). Las restricciones CHECK reflejan
/// RN-27. El identificador es el UUID de la entidad del dominio; que haya una sola fila lo garantiza
/// el índice único sobre una expresión constante.
/// </summary>
internal sealed class SystemSettingsConfiguration : IEntityTypeConfiguration<SystemSettings>
{
    private const string SingletonProperty = "Singleton";

    public void Configure(EntityTypeBuilder<SystemSettings> builder)
    {
        builder.ToTable("system_settings", table =>
        {
            table.HasCheckConstraint("ck_system_settings_company_name_not_empty", "length(trim(company_name)) > 0");
            table.HasCheckConstraint("ck_system_settings_vat_rate", "vat_rate BETWEEN 0 AND 100");
            table.HasCheckConstraint("ck_system_settings_tier1_threshold", "tier1_threshold > 0");
            table.HasCheckConstraint("ck_system_settings_tier2_threshold", "tier2_threshold > tier1_threshold");
            table.HasCheckConstraint("ck_system_settings_tier1_discount_rate", "tier1_discount_rate BETWEEN 0 AND 100");
            table.HasCheckConstraint(
                "ck_system_settings_tier2_discount_rate",
                "tier2_discount_rate BETWEEN 0 AND 100 AND tier2_discount_rate >= tier1_discount_rate");
            table.HasCheckConstraint("ck_system_settings_vip_discount_rate", "vip_discount_rate BETWEEN 0 AND 100");
            table.HasCheckConstraint(
                "ck_system_settings_max_discount",
                "tier2_discount_rate + vip_discount_rate <= 100");
            table.HasCheckConstraint(
                "ck_system_settings_validity_days",
                $"validity_days BETWEEN {SystemSettings.MinValidityDays} AND {SystemSettings.MaxValidityDays}");
        });

        builder.ConfigureAggregateRoot();

        // Datos de la empresa y condiciones por defecto.
        builder.Property(s => s.CompanyName)
            .HasMaxLength(SystemSettings.CompanyNameMaxLength)
            .IsRequired();
        builder.Property(s => s.CompanyLegalId).HasMaxLength(SystemSettings.CompanyLegalIdMaxLength);
        builder.Property(s => s.CompanyEmail).IsEmail();
        builder.Property(s => s.CompanyPhone).IsPhone();
        builder.Property(s => s.CompanyAddress).HasMaxLength(SystemSettings.CompanyAddressMaxLength);
        builder.Property(s => s.DefaultCommercialTerms).HasMaxLength(SystemSettings.DefaultCommercialTermsMaxLength);

        // Parámetros comerciales (RN-26, RN-27).
        builder.Property(s => s.VatPercentage).IsPercentage().HasColumnName("vat_rate").IsRequired();
        builder.Property(s => s.Tier1Threshold).IsMoney().HasColumnName("tier1_threshold").IsRequired();
        builder.Property(s => s.Tier1Discount).IsPercentage().HasColumnName("tier1_discount_rate").IsRequired();
        builder.Property(s => s.Tier2Threshold).IsMoney().HasColumnName("tier2_threshold").IsRequired();
        builder.Property(s => s.Tier2Discount).IsPercentage().HasColumnName("tier2_discount_rate").IsRequired();
        builder.Property(s => s.VipDiscount).IsPercentage().HasColumnName("vip_discount_rate").IsRequired();
        builder.Property(s => s.ValidityDays)
            .HasColumnType("smallint")
            .IsRequired();

        // Garantiza el registro único: columna constante con índice único.
        builder.Property<bool>(SingletonProperty)
            .HasColumnName("singleton")
            .HasDefaultValue(true)
            .ValueGeneratedOnAdd();
        builder.ToTable(table => table.HasCheckConstraint("ck_system_settings_singleton", "singleton"));
        builder.HasIndex(SingletonProperty)
            .IsUnique()
            .HasDatabaseName("uq_system_settings_singleton");
    }
}
