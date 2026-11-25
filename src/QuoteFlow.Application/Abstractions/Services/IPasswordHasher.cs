namespace QuoteFlow.Application.Abstractions.Services;

/// <summary>Cálculo y verificación de hashes de contraseña.</summary>
public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string passwordHash);
}
