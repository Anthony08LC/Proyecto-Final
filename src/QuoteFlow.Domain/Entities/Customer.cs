using QuoteFlow.Domain.Common;
using QuoteFlow.Domain.Exceptions;
using QuoteFlow.Domain.ValueObjects;

namespace QuoteFlow.Domain.Entities;

/// <summary>
/// Cliente al que se dirigen las cotizaciones. No se elimina físicamente: se desactiva (RN-20).
/// Que solo el Administrador cambie la condición VIP (RN-21) se valida en la capa de aplicación.
/// </summary>
public sealed class Customer : AuditableEntity
{
    public const int NameMaxLength = 150;
    public const int CompanyMaxLength = 150;
    public const int NotesMaxLength = 1000;

    public string Name { get; private set; }

    public string? Company { get; private set; }

    public Email? Email { get; private set; }

    public Phone? Phone { get; private set; }

    public bool IsVip { get; private set; }

    public string? Notes { get; private set; }

    public bool IsActive { get; private set; }

    private Customer(
        string name,
        string? company,
        Email? email,
        Phone? phone,
        bool isVip,
        string? notes)
    {
        Name = name;
        Company = company;
        Email = email;
        Phone = phone;
        IsVip = isVip;
        Notes = notes;
        IsActive = true;
    }

    /// <summary>Crea un cliente activo.</summary>
    public static Customer Create(
        string? name,
        string? company,
        Email? email,
        Phone? phone,
        string? notes,
        bool isVip = false)
    {
        return new Customer(
            Guard.Required(name, NameMaxLength, "nombre"),
            Guard.Optional(company, CompanyMaxLength, "empresa"),
            email,
            phone,
            isVip,
            Guard.Optional(notes, NotesMaxLength, "observaciones"));
    }

    /// <summary>Actualiza los datos de contacto y las observaciones.</summary>
    public void Update(
        string? name,
        string? company,
        Email? email,
        Phone? phone,
        string? notes)
    {
        Name = Guard.Required(name, NameMaxLength, "nombre");
        Company = Guard.Optional(company, CompanyMaxLength, "empresa");
        Email = email;
        Phone = phone;
        Notes = Guard.Optional(notes, NotesMaxLength, "observaciones");
    }

    public void MarkAsVip()
    {
        if (IsVip)
        {
            throw new DomainException("El cliente ya es VIP.");
        }

        IsVip = true;
    }

    public void RemoveVip()
    {
        if (!IsVip)
        {
            throw new DomainException("El cliente no es VIP.");
        }

        IsVip = false;
    }

    public void Deactivate()
    {
        if (!IsActive)
        {
            throw new DomainException("El cliente ya está inactivo.");
        }

        IsActive = false;
    }

    public void Reactivate()
    {
        if (IsActive)
        {
            throw new DomainException("El cliente ya está activo.");
        }

        IsActive = true;
    }
}
