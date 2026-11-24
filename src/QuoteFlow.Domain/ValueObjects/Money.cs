using QuoteFlow.Domain.Exceptions;

namespace QuoteFlow.Domain.ValueObjects;

/// <summary>
/// Monto en colones: no negativo, con 2 decimales como máximo.
/// Encapsula el redondeo a 2 decimales alejándose de cero (RN-17). Inmutable.
/// </summary>
public sealed record Money : IComparable<Money>
{
    public const int Decimals = 2;

    public static readonly Money Zero = new(0m);

    public decimal Amount { get; }

    private Money(decimal amount)
    {
        Amount = amount;
    }

    /// <summary>
    /// Crea un monto exacto; lanza <see cref="DomainException"/> si es negativo
    /// o tiene más de 2 decimales.
    /// </summary>
    public static Money Create(decimal amount)
    {
        EnsureNotNegative(amount);

        if (decimal.Round(amount, Decimals) != amount)
        {
            throw new DomainException(
                $"El monto no puede tener más de {Decimals} decimales.");
        }

        return new Money(amount);
    }

    /// <summary>Crea un monto redondeando a 2 decimales según RN-17.</summary>
    public static Money FromRounded(decimal amount)
    {
        var rounded = Round(amount);
        EnsureNotNegative(rounded);
        return new Money(rounded);
    }

    public Money Add(Money other) => new(Amount + other.Amount);

    /// <summary>Resta un monto; lanza <see cref="DomainException"/> si el resultado es negativo.</summary>
    public Money Subtract(Money other)
    {
        var result = Amount - other.Amount;

        if (result < 0m)
        {
            throw new DomainException("El resultado de la resta no puede ser un monto negativo.");
        }

        return new Money(result);
    }

    /// <summary>Multiplica por un factor no negativo y redondea según RN-17.</summary>
    public Money Multiply(decimal factor)
    {
        if (factor < 0m)
        {
            throw new DomainException("El factor de multiplicación no puede ser negativo.");
        }

        return FromRounded(Amount * factor);
    }

    /// <summary>Calcula el porcentaje indicado de este monto, redondeado según RN-17.</summary>
    public Money PercentageOf(Percentage percentage) =>
        FromRounded(Amount * percentage.Value / 100m);

    public int CompareTo(Money? other) =>
        other is null ? 1 : Amount.CompareTo(other.Amount);

    public static Money operator +(Money left, Money right) => left.Add(right);

    public static Money operator -(Money left, Money right) => left.Subtract(right);

    public static bool operator >(Money left, Money right) => left.CompareTo(right) > 0;

    public static bool operator <(Money left, Money right) => left.CompareTo(right) < 0;

    public static bool operator >=(Money left, Money right) => left.CompareTo(right) >= 0;

    public static bool operator <=(Money left, Money right) => left.CompareTo(right) <= 0;

    public override string ToString() => Amount.ToString("F2");

    private static decimal Round(decimal amount) =>
        decimal.Round(amount, Decimals, MidpointRounding.AwayFromZero);

    private static void EnsureNotNegative(decimal amount)
    {
        if (amount < 0m)
        {
            throw new DomainException("El monto no puede ser negativo.");
        }
    }
}
