using System.Globalization;
using Microsoft.EntityFrameworkCore;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Infrastructure.Persistence.Records;

namespace QuoteFlow.Infrastructure.Persistence;

/// <summary>
/// Consecutivo de cotizaciones <c>COT-AAAA-000001</c> (RN-13). Incrementa la fila del año con un
/// único <c>INSERT … ON CONFLICT DO UPDATE</c>, que la bloquea hasta el fin de la transacción:
/// las emisiones simultáneas reciben números distintos y consecutivos, y si la emisión falla el
/// incremento se revierte, por lo que no quedan saltos. La primera emisión del año crea la fila.
/// </summary>
internal sealed class QuoteNumberGenerator(ApplicationDbContext dbContext) : IQuoteNumberGenerator
{
    public async Task<string> NextAsync(int year, CancellationToken cancellationToken = default)
    {
        if (dbContext.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException(
                "El consecutivo de cotización solo puede generarse dentro de la transacción de emisión.");
        }

        if (year is < QuoteNumberSequence.MinYear or > QuoteNumberSequence.MaxYear)
        {
            throw new ArgumentOutOfRangeException(nameof(year), year, "El año del consecutivo no es válido.");
        }

        // Sin operadores LINQ: EF Core no envuelve la sentencia en una subconsulta.
        var numbers = await dbContext.Database
            .SqlQuery<int>($"""
                INSERT INTO quote_number_sequences (year, last_number)
                VALUES ({(short)year}, 1)
                ON CONFLICT (year) DO UPDATE
                    SET last_number = quote_number_sequences.last_number + 1
                RETURNING last_number AS "Value"
                """)
            .ToListAsync(cancellationToken);

        var next = numbers.Single();

        if (next > QuoteNumberSequence.MaxNumber)
        {
            throw new InvalidOperationException(
                $"Se agotó el consecutivo de cotizaciones del año {year}.");
        }

        return string.Create(CultureInfo.InvariantCulture, $"COT-{year:D4}-{next:D6}");
    }
}
