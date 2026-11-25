using Microsoft.AspNetCore.Identity;
using QuoteFlow.Application.Abstractions.Services;

namespace QuoteFlow.Infrastructure.Security;

/// <summary>
/// Hash de contraseñas con el algoritmo estándar de ASP.NET Core Identity: PBKDF2 con HMAC-SHA512,
/// sal aleatoria e iteraciones configurables (RNF-SEG-02). Las contraseñas nunca se guardan ni
/// se registran en texto plano.
/// </summary>
internal sealed class PasswordHasher : IPasswordHasher
{
    private static readonly HashSubject Subject = new();
    private readonly PasswordHasher<HashSubject> _hasher = new();

    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrEmpty(password);
        return _hasher.HashPassword(Subject, password);
    }

    public bool Verify(string password, string passwordHash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordHash))
        {
            return false;
        }

        try
        {
            return _hasher.VerifyHashedPassword(Subject, passwordHash, password)
                is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
        }
        catch (FormatException)
        {
            // Un hash con formato inválido equivale a credenciales incorrectas.
            return false;
        }
    }

    /// <summary>El algoritmo no usa datos del usuario; se requiere solo por la firma genérica.</summary>
    private sealed class HashSubject;
}
