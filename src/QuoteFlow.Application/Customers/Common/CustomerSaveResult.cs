namespace QuoteFlow.Application.Customers.Common;

/// <summary>
/// Resultado de registrar o editar un cliente.
/// <paramref name="PossibleDuplicateEmail"/> indica que otro cliente ya usa el correo:
/// se advierte al usuario, pero la operación se completa (RF-CLI-01).
/// </summary>
public sealed record CustomerSaveResult(Guid CustomerId, bool PossibleDuplicateEmail);
