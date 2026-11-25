namespace QuoteFlow.Application.Common.Models;

/// <summary>Parámetros de paginación de una consulta. Los valores fuera de rango se ajustan.</summary>
public sealed record PageRequest
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public int Page { get; }

    public int PageSize { get; }

    public PageRequest(int page = 1, int pageSize = DefaultPageSize)
    {
        Page = Math.Max(1, page);
        PageSize = Math.Clamp(pageSize, 1, MaxPageSize);
    }

    public int Skip => (Page - 1) * PageSize;
}
