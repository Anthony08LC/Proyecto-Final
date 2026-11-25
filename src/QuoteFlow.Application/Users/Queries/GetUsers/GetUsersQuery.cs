using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Users.Common;
using QuoteFlow.Domain.Enums;

namespace QuoteFlow.Application.Users.Queries.GetUsers;

/// <summary>
/// Lista y busca usuarios por nombre o correo, con filtros por rol y estado (RF-USR-01).
/// Solo Administrador. Resultado paginado; el usuario de sistema no aparece.
/// </summary>
public sealed record GetUsersQuery(
    string? Search = null,
    UserRole? Role = null,
    bool? IsActive = null,
    int Page = 1,
    int PageSize = PageRequest.DefaultPageSize) : IQuery<PagedResult<UserDto>>;
