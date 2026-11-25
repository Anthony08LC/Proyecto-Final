using QuoteFlow.Application.Common.Validation;

namespace QuoteFlow.Application.Users.Common;

/// <summary>
/// Política de contraseñas (RNF-SEG-03): mínimo 10 caracteres, con al menos una letra y un número.
/// </summary>
internal static class PasswordPolicy
{
    public const int MinLength = 10;
    public const int MaxLength = 128;

    /// <summary>Registra en <paramref name="errors"/> cada regla que la contraseña no cumple.</summary>
    public static void Validate(ValidationErrors errors, string field, string? password)
    {
        if (string.IsNullOrEmpty(password))
        {
            errors.Add(field, "La contraseña es obligatoria.");
            return;
        }

        if (password.Length < MinLength)
        {
            errors.Add(field, $"La contraseña debe tener al menos {MinLength} caracteres.");
        }

        if (password.Length > MaxLength)
        {
            errors.Add(field, $"La contraseña no puede superar los {MaxLength} caracteres.");
        }

        if (!password.Any(char.IsLetter))
        {
            errors.Add(field, "La contraseña debe contener al menos una letra.");
        }

        if (!password.Any(char.IsDigit))
        {
            errors.Add(field, "La contraseña debe contener al menos un número.");
        }
    }
}
