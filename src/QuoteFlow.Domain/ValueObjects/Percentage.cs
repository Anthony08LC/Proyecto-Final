using QuoteFlow.Domain.Exceptions;

namespace QuoteFlow.Domain.ValueObjects;

/// <summary>
/// Porcentaje entre 0 y 100, con 2 decimales como máximo. Inmutable.
/// </summary>
public sealed record Percentage : IComparable<Percentage>
{
    public const decimal Min = 0m;
    public const decimal Max = 100m;
    public const int Decimals = 2;

    public static readonly Percentage Zero = new(Min);

    public decimal Value { get; }

    private Percentage(decimal value)
    {
        Value = value;
    }

    /// <summary>
    /// Crea un porcentaje; lanza <see cref="DomainException"/> si está fuera del rango 0–100
    /// o tiene más de 2 decimales.
    /// </summary>
    public static Percentage Create(decimal value)
    {
        if (value < Min || value > Max)
        {
            throw new DomainException(
                $"El porcentaje debe estar entre {Min} y {Max}.");
        }

        if (decimal.Round(value, Decimals) != value)
        {
            throw new DomainException(
                $"El porcentaje no puede tener más de {Decimals} decimales.");
        }

        return new Percentage(value);
    }

    /// <summary>Suma dos porcentajes; lanza <see cref="DomainException"/> si el resultado supera 100.</summary>
    public Percentage Add(Percentage other)
    {
        var result = Value + other.Value;

        if (result > Max)
        {
            throw new DomainException(
                $"La suma de los porcentajes no puede superar {Max}.");
        }

        return new Percentage(result);
    }

    public int CompareTo(Percentage? other) =>
        other is null ? 1 : Value.CompareTo(other.Value);

    public static Percentage operator +(Percentage left, Percentage right) => left.Add(right);

    public static bool operator >(Percentage left, Percentage right) => left.CompareTo(right) > 0;

    public static bool operator <(Percentage left, Percentage right) => left.CompareTo(right) < 0;

    public static bool operator >=(Percentage left, Percentage right) => left.CompareTo(right) >= 0;

    public static bool operator <=(Percentage left, Percentage right) => left.CompareTo(right) <= 0;

    public override string ToString() => $"{Value:0.##} %";
}
