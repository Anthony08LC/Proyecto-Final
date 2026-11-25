using Microsoft.EntityFrameworkCore;
using QuoteFlow.Application.Common.Models;

namespace QuoteFlow.Infrastructure.Persistence.Repositories;

internal static class QueryExtensions
{
    /// <summary>Ejecuta la consulta paginada: total de elementos y la página solicitada.</summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        PageRequest page,
        CancellationToken cancellationToken)
    {
        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip(page.Skip)
            .Take(page.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>(items, page.Page, page.PageSize, total);
    }
}

/// <summary>Patrones para <c>ILIKE</c>: escapa los comodines del texto buscado.</summary>
internal static class LikePattern
{
    /// <summary>Patrón «contiene» sin comodines del usuario (<c>%</c>, <c>_</c> y <c>\</c> se escapan).</summary>
    public static string Contains(string text) => $"%{Escape(text)}%";

    private static string Escape(string text) =>
        text.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_");
}
