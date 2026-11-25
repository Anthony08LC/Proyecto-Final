using QuoteFlow.Application.Common.Validation;
using QuoteFlow.Domain.Entities;
using QuoteFlow.Domain.Enums;
using QuoteFlow.Domain.ValueObjects;

namespace QuoteFlow.Application.Users.Common;

/// <summary>Datos de entrada de un usuario ya validados (RF-USR-02, RF-USR-03).</summary>
internal sealed record UserInput(string FullName, Email Email, UserRole Role)
{
    public static UserInput Parse(
        ValidationErrors errors,
        string? fullName,
        string? email,
        UserRole role)
    {
        var parsedName = errors.Required("fullName", "El nombre", fullName, User.FullNameMaxLength);
        var parsedEmail = errors.Capture("email", () => Email.Create(email));

        if (!Enum.IsDefined(role))
        {
            errors.Add("role", "El rol indicado no es válido.");
        }

        return new UserInput(parsedName ?? string.Empty, parsedEmail!, role);
    }
}
