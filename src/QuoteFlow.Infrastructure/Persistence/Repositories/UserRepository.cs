using Microsoft.EntityFrameworkCore;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Domain.Entities;
using QuoteFlow.Domain.Enums;
using QuoteFlow.Domain.ValueObjects;

namespace QuoteFlow.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository(ApplicationDbContext dbContext)
    : Repository<User>(dbContext), IUserRepository
{
    public Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default) =>
        Set.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public Task<bool> EmailExistsAsync(
        Email email,
        Guid? excludingId = null,
        CancellationToken cancellationToken = default) =>
        Set.AnyAsync(
            u => u.Email == email && (excludingId == null || u.Id != excludingId),
            cancellationToken);

    public Task<int> CountActiveAdministratorsAsync(CancellationToken cancellationToken = default) =>
        Set.CountAsync(
            u => u.Role == UserRole.Administrator && u.IsActive && !u.IsSystem,
            cancellationToken);

    public Task<PagedResult<User>> SearchAsync(
        UserSearchCriteria criteria,
        PageRequest page,
        CancellationToken cancellationToken = default)
    {
        IQueryable<User> query;

        if (criteria.Search is { } search)
        {
            // El correo es un value object con conversión: la búsqueda por texto se hace en SQL.
            // Se selecciona xmin explícitamente porque «*» no incluye columnas de sistema.
            var pattern = LikePattern.Contains(search);
            query = Set.FromSql(
                $"SELECT u.*, u.xmin FROM users AS u WHERE u.full_name ILIKE {pattern} OR u.email ILIKE {pattern}");
        }
        else
        {
            query = Set;
        }

        query = query.Where(u => !u.IsSystem);

        if (criteria.Role is { } role)
        {
            query = query.Where(u => u.Role == role);
        }

        if (criteria.IsActive is { } isActive)
        {
            query = query.Where(u => u.IsActive == isActive);
        }

        return query
            .OrderBy(u => u.FullName)
            .ThenBy(u => u.Id)
            .ToPagedResultAsync(page, cancellationToken);
    }

    public Task<IReadOnlyList<User>> GetByIdsAsync(
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default) =>
        FindByIdsAsync(ids, cancellationToken);
}
