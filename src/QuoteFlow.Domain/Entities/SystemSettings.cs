using QuoteFlow.Domain.Common;
using QuoteFlow.Domain.Exceptions;
using QuoteFlow.Domain.ValueObjects;

namespace QuoteFlow.Domain.Entities;

/// <summary>
/// Parámetros comerciales del sistema (RN-26). Valida en conjunto las reglas de RN-27:
/// umbral 2 &gt; umbral 1 &gt; 0; % tramo 2 ≥ % tramo 1; % tramo 2 + % VIP ≤ 100;
/// vigencia entre 1 y 365 días. Los porcentajes ya se garantizan entre 0 y 100 por <see cref="Percentage"/>.
/// </summary>
public sealed class SystemSettings : AuditableEntity
{
    public const int MinValidityDays = 1;
    public const int MaxValidityDays = 365;
    public const int CompanyNameMaxLength = 150;
    public const int CompanyLegalIdMaxLength = 20;
    public const int CompanyAddressMaxLength = 300;
    public const int DefaultCommercialTermsMaxLength = 4000;
    public const string DefaultCompanyName = "TechSolutions CR";

    // Datos de la empresa y condiciones por defecto (RF-CFG-02).
    public string CompanyName { get; private set; } = DefaultCompanyName;

    public string? CompanyLegalId { get; private set; }

    public Email? CompanyEmail { get; private set; }

    public Phone? CompanyPhone { get; private set; }

    public string? CompanyAddress { get; private set; }

    /// <summary>Texto con el que se inicializan las condiciones comerciales de cada borrador nuevo.</summary>
    public string? DefaultCommercialTerms { get; private set; }

    public Percentage VatPercentage { get; private set; }

    public Money Tier1Threshold { get; private set; }

    public Percentage Tier1Discount { get; private set; }

    public Money Tier2Threshold { get; private set; }

    public Percentage Tier2Discount { get; private set; }

    public Percentage VipDiscount { get; private set; }

    public int ValidityDays { get; private set; }

    private SystemSettings(
        Percentage vatPercentage,
        Money tier1Threshold,
        Percentage tier1Discount,
        Money tier2Threshold,
        Percentage tier2Discount,
        Percentage vipDiscount,
        int validityDays)
    {
        VatPercentage = vatPercentage;
        Tier1Threshold = tier1Threshold;
        Tier1Discount = tier1Discount;
        Tier2Threshold = tier2Threshold;
        Tier2Discount = tier2Discount;
        VipDiscount = vipDiscount;
        ValidityDays = validityDays;
    }

    /// <summary>Crea la configuración validando todas las reglas de RN-27.</summary>
    public static SystemSettings Create(
        Percentage vatPercentage,
        Money tier1Threshold,
        Percentage tier1Discount,
        Money tier2Threshold,
        Percentage tier2Discount,
        Percentage vipDiscount,
        int validityDays)
    {
        EnsureRequired(vatPercentage, "porcentaje de IVA");
        EnsureCommercialPolicy(tier1Threshold, tier1Discount, tier2Threshold, tier2Discount, vipDiscount);
        EnsureValidityDays(validityDays);

        return new SystemSettings(
            vatPercentage,
            tier1Threshold,
            tier1Discount,
            tier2Threshold,
            tier2Discount,
            vipDiscount,
            validityDays);
    }

    /// <summary>
    /// Crea la configuración con los valores iniciales de la especificación:
    /// IVA 13 %, tramo 1 más de ₡500.000 → 5 %, tramo 2 más de ₡1.000.000 → 10 %, VIP 3 %, vigencia 30 días.
    /// </summary>
    public static SystemSettings CreateDefault() =>
        Create(
            Percentage.Create(13m),
            Money.Create(500_000m),
            Percentage.Create(5m),
            Money.Create(1_000_000m),
            Percentage.Create(10m),
            Percentage.Create(3m),
            30);

    /// <summary>
    /// Actualiza los datos de la empresa y las condiciones comerciales por defecto.
    /// El nombre de la empresa es obligatorio; correo y teléfono ya vienen validados.
    /// </summary>
    public void UpdateCompanyInfo(
        string? companyName,
        string? companyLegalId,
        Email? companyEmail,
        Phone? companyPhone,
        string? companyAddress,
        string? defaultCommercialTerms)
    {
        var name = Guard.Required(companyName, CompanyNameMaxLength, "nombre de la empresa");
        var legalId = Guard.Optional(companyLegalId, CompanyLegalIdMaxLength, "identificación de la empresa");
        var address = Guard.Optional(companyAddress, CompanyAddressMaxLength, "dirección de la empresa");
        var terms = Guard.Optional(
            defaultCommercialTerms,
            DefaultCommercialTermsMaxLength,
            "condiciones comerciales por defecto");

        CompanyName = name;
        CompanyLegalId = legalId;
        CompanyEmail = companyEmail;
        CompanyPhone = companyPhone;
        CompanyAddress = address;
        DefaultCommercialTerms = terms;
    }

    public void UpdateVatPercentage(Percentage vatPercentage)
    {
        EnsureRequired(vatPercentage, "porcentaje de IVA");
        VatPercentage = vatPercentage;
    }

    /// <summary>
    /// Actualiza los tramos de descuento y el porcentaje VIP en conjunto,
    /// porque sus reglas dependen unas de otras.
    /// </summary>
    public void UpdateDiscountPolicy(
        Money tier1Threshold,
        Percentage tier1Discount,
        Money tier2Threshold,
        Percentage tier2Discount,
        Percentage vipDiscount)
    {
        EnsureCommercialPolicy(tier1Threshold, tier1Discount, tier2Threshold, tier2Discount, vipDiscount);

        Tier1Threshold = tier1Threshold;
        Tier1Discount = tier1Discount;
        Tier2Threshold = tier2Threshold;
        Tier2Discount = tier2Discount;
        VipDiscount = vipDiscount;
    }

    public void UpdateValidityDays(int validityDays)
    {
        EnsureValidityDays(validityDays);
        ValidityDays = validityDays;
    }

    /// <summary>Verifica que los valores actuales cumplan RN-27.</summary>
    internal void EnsureValidCommercialPolicy()
    {
        EnsureRequired(VatPercentage, "porcentaje de IVA");
        EnsureCommercialPolicy(Tier1Threshold, Tier1Discount, Tier2Threshold, Tier2Discount, VipDiscount);
        EnsureValidityDays(ValidityDays);
    }

    private static void EnsureCommercialPolicy(
        Money? tier1Threshold,
        Percentage? tier1Discount,
        Money? tier2Threshold,
        Percentage? tier2Discount,
        Percentage? vipDiscount)
    {
        EnsureRequired(tier1Threshold, "umbral del tramo 1");
        EnsureRequired(tier1Discount, "descuento del tramo 1");
        EnsureRequired(tier2Threshold, "umbral del tramo 2");
        EnsureRequired(tier2Discount, "descuento del tramo 2");
        EnsureRequired(vipDiscount, "descuento VIP");

        if (tier1Threshold! <= Money.Zero)
        {
            throw new DomainException("El umbral del tramo 1 debe ser mayor que cero.");
        }

        if (tier2Threshold! <= tier1Threshold!)
        {
            throw new DomainException(
                "El umbral del tramo 2 debe ser mayor que el umbral del tramo 1.");
        }

        if (tier2Discount! < tier1Discount!)
        {
            throw new DomainException(
                "El descuento del tramo 2 debe ser mayor o igual que el descuento del tramo 1.");
        }

        if (tier2Discount!.Value + vipDiscount!.Value > Percentage.Max)
        {
            throw new DomainException(
                $"La suma del descuento del tramo 2 y el descuento VIP no puede superar {Percentage.Max} %.");
        }
    }

    private static void EnsureValidityDays(int validityDays)
    {
        if (validityDays < MinValidityDays || validityDays > MaxValidityDays)
        {
            throw new DomainException(
                $"La vigencia debe estar entre {MinValidityDays} y {MaxValidityDays} días.");
        }
    }

    private static void EnsureRequired(object? value, string fieldName)
    {
        if (value is null)
        {
            throw new DomainException($"El campo {fieldName} es obligatorio.");
        }
    }
}
