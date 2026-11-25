using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using QuoteFlow.Domain.ValueObjects;

namespace QuoteFlow.Infrastructure.Persistence.Configurations;

/// <summary>
/// Conversiones entre los value objects del dominio y sus columnas. Al leer se reconstruyen con
/// sus fábricas, por lo que un valor inválido en la base de datos se detecta de inmediato.
/// EF Core no invoca el convertidor para valores nulos, así que sirven también para propiedades opcionales.
/// </summary>
internal static class ValueObjectConverters
{
    public static readonly ValueConverter<Email, string> Email =
        new(email => email.Value, value => Domain.ValueObjects.Email.Create(value));

    public static readonly ValueConverter<Phone, string> Phone =
        new(phone => phone.Value, value => Domain.ValueObjects.Phone.Create(value));

    public static readonly ValueConverter<Money, decimal> Money =
        new(money => money.Amount, value => Domain.ValueObjects.Money.Create(value));

    public static readonly ValueConverter<Percentage, decimal> Percentage =
        new(percentage => percentage.Value, value => Domain.ValueObjects.Percentage.Create(value));
}

/// <summary>Tipos de columna de la sección 9.1 de la especificación.</summary>
internal static class ColumnTypes
{
    /// <summary>Montos: <c>numeric(18,2)</c>.</summary>
    public const int MoneyPrecision = 18;

    /// <summary>Porcentajes: <c>numeric(5,2)</c>.</summary>
    public const int PercentagePrecision = 5;

    /// <summary>Cantidades: <c>numeric(12,2)</c>.</summary>
    public const int QuantityPrecision = 12;

    public const int Scale = 2;
}
