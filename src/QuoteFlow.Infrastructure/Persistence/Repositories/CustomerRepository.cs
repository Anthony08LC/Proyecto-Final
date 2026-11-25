using Microsoft.EntityFrameworkCore;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Domain.Entities;
using QuoteFlow.Domain.ValueObjects;

namespace QuoteFlow.Infrastructure.Persistence.Repositories;

internal sealed class CustomerRepository(ApplicationDbContext dbContext)
    : Repository<Customer>(dbContext), ICustomerRepository
{
    public Task<bool> EmailInUseAsync(
        Email email,
        Guid? excludingId = null,
        CancellationToken cancellationToken = default) =>
        Set.AnyAsync(
            c => c.Email == email && (excludingId == null || c.Id != excludingId),
            cancellationToken);

    public Task<PagedResult<Customer>> SearchAsync(
        CustomerSearchCriteria criteria,
        PageRequest page,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Customer> query;

        if (criteria.Search is { } search)
        {
            // El correo es un value object con conversión: la búsqueda por texto se hace en SQL.
            // Se selecciona xmin explícitamente porque «*» no incluye columnas de sistema.
            var pattern = LikePattern.Contains(search);
            query = Set.FromSql(
                $"SELECT c.*, c.xmin FROM customers AS c WHERE c.name ILIKE {pattern} OR c.company ILIKE {pattern} OR c.email ILIKE {pattern}");
        }
        else
        {
            query = Set;
        }

        if (criteria.IsActive is { } isActive)
        {
            query = query.Where(c => c.IsActive == isActive);
        }

        if (criteria.IsVip is { } isVip)
        {
            query = query.Where(c => c.IsVip == isVip);
        }

        return query
            .OrderBy(c => c.Name)
            .ThenBy(c => c.Id)
            .ToPagedResultAsync(page, cancellationToken);
    }

    public Task<IReadOnlyList<Customer>> GetByIdsAsync(
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default) =>
        FindByIdsAsync(ids, cancellationToken);
}
