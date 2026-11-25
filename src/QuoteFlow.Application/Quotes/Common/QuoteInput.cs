using QuoteFlow.Application.Common.Exceptions;
using QuoteFlow.Domain.Entities;

namespace QuoteFlow.Application.Quotes.Common;

/// <summary>Validaciones de entrada propias de las cotizaciones.</summary>
internal static class QuoteInput
{
    /// <summary>La cantidad es mayor que 0, con 2 decimales como máximo (RN-07).</summary>
    public static void EnsureValidQuantity(decimal quantity)
    {
        if (quantity <= 0m)
        {
            throw new ValidationException("quantity", "La cantidad debe ser mayor que cero.");
        }

        if (decimal.Round(quantity, QuoteLine.QuantityDecimals) != quantity)
        {
            throw new ValidationException(
                "quantity",
                $"La cantidad no puede tener más de {QuoteLine.QuantityDecimals} decimales.");
        }
    }
}
