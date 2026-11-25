using QuoteFlow.Application.Common.Models;
using QuoteFlow.Domain.Entities;

namespace QuoteFlow.Application.Abstractions.Services;

/// <summary>
/// Emite el token de sesión: JWT de 8 horas con el rol y el sello de seguridad (RNF-SEG-04).
/// </summary>
public interface IAccessTokenGenerator
{
    AccessToken Generate(User user);
}
