using System.Globalization;
using System.Text.RegularExpressions;
using QuoteFlow.Domain.Common;
using QuoteFlow.Domain.Enums;
using QuoteFlow.Domain.Exceptions;
using QuoteFlow.Domain.Services;
using QuoteFlow.Domain.ValueObjects;

namespace QuoteFlow.Domain.Entities;

/// <summary>
/// Cotización comercial: raíz del agregado que contiene sus <see cref="QuoteLine"/>.
/// Un borrador se modifica y recalcula libremente (RN-22); al emitir se congelan
/// líneas, datos del cliente, porcentajes y totales (RN-12), y desde entonces
/// solo puede cambiar de estado (RN-10, RN-15).
/// </summary>
public sealed partial class Quote : AuditableEntity
{
    public const int CloseReasonMaxLength = 500;
    public const int TextMaxLength = 4000;

    private readonly List<QuoteLine> _lines = [];

    public string? Number { get; private set; }

    public Guid CustomerId { get; private set; }

    public Guid OwnerId { get; private set; }

    /// <summary>Cotización de la que esta es copia (RN-24); nula si no es una copia.</summary>
    public Guid? SourceQuoteId { get; private set; }

    public QuoteStatus Status { get; private set; }

    public IReadOnlyList<QuoteLine> Lines => _lines.AsReadOnly();

    // Textos comerciales (opcionales, máximo 4000 caracteres cada uno).
    public string? Introduction { get; private set; }

    public string? ExecutiveSummary { get; private set; }

    public string? Notes { get; private set; }

    public string? CommercialTerms { get; private set; }

    // Datos del cliente congelados al emitir (RN-12).
    public string? CustomerName { get; private set; }

    public string? CustomerCompany { get; private set; }

    public Email? CustomerEmail { get; private set; }

    public Phone? CustomerPhone { get; private set; }

    public bool? CustomerIsVip { get; private set; }

    /// <summary>Desglose financiero: estimado en borrador, congelado al emitir.</summary>
    public QuoteBreakdown Breakdown { get; private set; } = QuoteBreakdown.Empty;

    // Ciclo de vida.
    public DateTimeOffset? IssuedAt { get; private set; }

    /// <summary>Fecha de vencimiento en hora de Costa Rica; válida hasta el final de ese día (RN-14).</summary>
    public DateOnly? ValidUntil { get; private set; }

    public DateTimeOffset? ClosedAt { get; private set; }

    public string? CloseReason { get; private set; }

    private Quote(Guid customerId, Guid ownerId)
    {
        CustomerId = customerId;
        OwnerId = ownerId;
        Status = QuoteStatus.Draft;
    }

    /// <summary>
    /// Crea un borrador para un cliente activo (RN-33). Las condiciones comerciales se
    /// inicializan con el texto por defecto de la configuración (RF-COT-01).
    /// </summary>
    public static Quote Create(Customer customer, Guid ownerId, string? defaultCommercialTerms = null)
    {
        EnsureActiveCustomer(customer);

        if (ownerId == Guid.Empty)
        {
            throw new DomainException("El propietario de la cotización es obligatorio.");
        }

        return new Quote(customer.Id, ownerId)
        {
            CommercialTerms = Guard.Optional(defaultCommercialTerms, TextMaxLength, "condiciones comerciales")
        };
    }

    /// <summary>
    /// Crea un borrador nuevo a partir de esta cotización, en cualquier estado (RN-23, RN-24, RN-25):
    /// mismo cliente (que debe estar activo), mismas líneas y textos, precios del catálogo actual
    /// y desglose recalculado con la política vigente. Las líneas con productos inactivos se omiten
    /// y se informan. La cotización original no cambia.
    /// </summary>
    public QuoteDuplication Duplicate(
        Customer customer,
        Guid ownerId,
        IEnumerable<Product> products,
        IEnumerable<Category> categories,
        SystemSettings settings)
    {
        EnsureCurrentCustomer(customer);

        if (!customer.IsActive)
        {
            throw new DomainException("No se puede duplicar: el cliente está inactivo.");
        }

        var copy = Create(customer, ownerId, CommercialTerms);
        copy.SourceQuoteId = Id;
        copy.Introduction = Introduction;
        copy.ExecutiveSummary = ExecutiveSummary;
        copy.Notes = Notes;

        var productsById = products.ToDictionary(p => p.Id);
        var categoriesById = categories.ToDictionary(c => c.Id);
        var omitted = new List<string>();

        foreach (var line in _lines.OrderBy(l => l.LineNumber))
        {
            if (!productsById.TryGetValue(line.ProductId, out var product) || !product.IsActive)
            {
                omitted.Add(line.ProductName);
                continue;
            }

            if (!categoriesById.TryGetValue(product.CategoryId, out var category))
            {
                throw new DomainException(
                    $"No se encontró la categoría del producto «{product.Name}».");
            }

            copy.AddLine(product, category, line.Quantity);
        }

        copy.Recalculate(customer, settings);

        return new QuoteDuplication(copy, omitted);
    }

    // ---------------------------------------------------------------
    // Edición del borrador
    // ---------------------------------------------------------------

    /// <summary>Cambia el cliente del borrador; el nuevo cliente debe estar activo.</summary>
    public void ChangeCustomer(Customer customer)
    {
        EnsureDraft();
        EnsureActiveCustomer(customer);
        CustomerId = customer.Id;
    }

    /// <summary>
    /// Actualiza los cuatro textos comerciales (RF-COT-05). Todos son opcionales,
    /// con un máximo de 4000 caracteres cada uno. Solo en borradores.
    /// </summary>
    public void UpdateTexts(
        string? introduction,
        string? executiveSummary,
        string? notes,
        string? commercialTerms)
    {
        EnsureDraft();

        var newIntroduction = Guard.Optional(introduction, TextMaxLength, "introducción");
        var newExecutiveSummary = Guard.Optional(executiveSummary, TextMaxLength, "resumen ejecutivo");
        var newNotes = Guard.Optional(notes, TextMaxLength, "observaciones");
        var newCommercialTerms = Guard.Optional(commercialTerms, TextMaxLength, "condiciones comerciales");

        Introduction = newIntroduction;
        ExecutiveSummary = newExecutiveSummary;
        Notes = newNotes;
        CommercialTerms = newCommercialTerms;
    }

    /// <summary>
    /// Agrega un producto activo. Si ya está en la cotización, suma la cantidad
    /// a la línea existente (RN-28).
    /// </summary>
    public void AddLine(
        Product product,
        Category category,
        decimal quantity,
        Percentage? discountPercentage = null)
    {
        EnsureDraft();

        if (product is null)
        {
            throw new DomainException("El producto es obligatorio.");
        }

        if (!product.IsActive)
        {
            throw new DomainException($"El producto «{product.Name}» está inactivo.");
        }

        var existing = FindLine(product.Id);

        if (existing is not null)
        {
            existing.IncreaseQuantity(quantity);

            if (discountPercentage is not null)
            {
                existing.ChangeDiscountPercentage(discountPercentage);
            }

            return;
        }

        _lines.Add(QuoteLine.Create(product, category, quantity, _lines.Count + 1, discountPercentage));
    }

    public void ChangeLineQuantity(Guid productId, decimal quantity)
    {
        EnsureDraft();
        GetLine(productId).ChangeQuantity(quantity);
    }

    public void ChangeLineDiscount(Guid productId, Percentage discountPercentage)
    {
        EnsureDraft();
        GetLine(productId).ChangeDiscountPercentage(discountPercentage);
    }

    /// <summary>Quita una línea y renumera las restantes.</summary>
    public void RemoveLine(Guid productId)
    {
        EnsureDraft();
        _lines.Remove(GetLine(productId));

        for (var i = 0; i < _lines.Count; i++)
        {
            _lines[i].ChangeLineNumber(i + 1);
        }
    }

    /// <summary>
    /// Actualiza precios y datos de las líneas con el catálogo vigente (RN-22).
    /// Cada producto de las líneas y su categoría deben venir en las colecciones.
    /// </summary>
    public void RefreshCatalog(IEnumerable<Product> products, IEnumerable<Category> categories)
    {
        EnsureDraft();

        var productsById = products.ToDictionary(p => p.Id);
        var categoriesById = categories.ToDictionary(c => c.Id);

        foreach (var line in _lines)
        {
            if (!productsById.TryGetValue(line.ProductId, out var product))
            {
                throw new DomainException($"No se encontró el producto «{line.ProductName}».");
            }

            if (!categoriesById.TryGetValue(product.CategoryId, out var category))
            {
                throw new DomainException(
                    $"No se encontró la categoría del producto «{product.Name}».");
            }

            line.RefreshFromCatalog(product, category);
        }
    }

    // ---------------------------------------------------------------
    // Cálculo
    // ---------------------------------------------------------------

    /// <summary>
    /// Recalcula el desglose del borrador con <see cref="QuoteCalculator"/>, usando la condición
    /// VIP del cliente y la configuración vigentes (RN-22).
    /// </summary>
    public void Recalculate(Customer customer, SystemSettings settings)
    {
        EnsureDraft();
        Breakdown = QuoteCalculator.Calculate(this, customer, settings);
    }

    // ---------------------------------------------------------------
    // Ciclo de vida (RN-15)
    // ---------------------------------------------------------------

    /// <summary>
    /// Emite el borrador: valida RN-09 y RN-16, recalcula por última vez con el catálogo
    /// y la configuración vigentes, congela los datos (RN-12), asigna el número (RN-13)
    /// y la fecha de vencimiento (RN-14).
    /// </summary>
    public void Emit(
        string number,
        Customer customer,
        IReadOnlyCollection<Product> products,
        IReadOnlyCollection<Category> categories,
        SystemSettings settings,
        DateTimeOffset now)
    {
        EnsureCanEmit(customer, products);

        var issueDate = CostaRicaTime.ToLocalDate(now);
        EnsureValidNumber(number, issueDate.Year);

        RefreshCatalog(products, categories);
        Recalculate(customer, settings);

        CustomerName = customer.Name;
        CustomerCompany = customer.Company;
        CustomerEmail = customer.Email;
        CustomerPhone = customer.Phone;
        CustomerIsVip = customer.IsVip;

        Number = number;
        IssuedAt = now;
        ValidUntil = issueDate.AddDays(settings.ValidityDays);
        Status = QuoteStatus.Issued;
    }

    /// <summary>
    /// Verifica las condiciones de emisión (RN-09, RN-16) sin modificar la cotización. Permite
    /// validar antes de reservar el número consecutivo, para que una emisión rechazada no lo consuma.
    /// </summary>
    public void EnsureCanEmit(Customer customer, IReadOnlyCollection<Product> products)
    {
        EnsureStatus(QuoteStatus.Draft, "emitir");

        if (_lines.Count == 0)
        {
            throw new DomainException("No se puede emitir una cotización sin líneas.");
        }

        EnsureCurrentCustomer(customer);

        if (!customer.IsActive)
        {
            throw new DomainException("No se puede emitir: el cliente está inactivo.");
        }

        var productsById = products.ToDictionary(p => p.Id);

        foreach (var line in _lines)
        {
            if (!productsById.TryGetValue(line.ProductId, out var product))
            {
                throw new DomainException($"No se encontró el producto «{line.ProductName}».");
            }

            if (!product.IsActive)
            {
                throw new DomainException($"No se puede emitir: el producto «{product.Name}» está inactivo.");
            }
        }
    }

    /// <summary>Registra la aceptación: solo desde Emitida y dentro de la vigencia (RN-35).</summary>
    public void Accept(DateTimeOffset now)
    {
        EnsureStatus(QuoteStatus.Issued, "aceptar");

        if (!IsWithinValidity(now))
        {
            throw new DomainException("No se puede aceptar: la cotización ya superó su vigencia.");
        }

        Close(QuoteStatus.Accepted, now, reason: null);
    }

    /// <summary>Registra el rechazo: solo desde Emitida; el motivo es opcional.</summary>
    public void Reject(DateTimeOffset now, string? reason = null)
    {
        EnsureStatus(QuoteStatus.Issued, "rechazar");
        Close(QuoteStatus.Rejected, now, Guard.Optional(reason, CloseReasonMaxLength, "motivo"));
    }

    /// <summary>
    /// Cancela un borrador o una cotización emitida. Si fue emitida, el motivo es obligatorio (RN-34).
    /// </summary>
    public void Cancel(DateTimeOffset now, string? reason = null)
    {
        var closeReason = Status switch
        {
            QuoteStatus.Draft => Guard.Optional(reason, CloseReasonMaxLength, "motivo"),
            QuoteStatus.Issued => Guard.Required(reason, CloseReasonMaxLength, "motivo"),
            _ => throw new DomainException(
                $"No se puede cancelar una cotización en estado {Status}.")
        };

        Close(QuoteStatus.Cancelled, now, closeReason);
    }

    /// <summary>Vence una cotización emitida cuya fecha de vencimiento ya pasó (RF-COT-11).</summary>
    public void Expire(DateTimeOffset now)
    {
        EnsureStatus(QuoteStatus.Issued, "vencer");

        if (IsWithinValidity(now))
        {
            throw new DomainException("No se puede vencer: la cotización aún está vigente.");
        }

        Close(QuoteStatus.Expired, now, reason: null);
    }

    /// <summary>
    /// Indica si la cotización sigue vigente: es válida hasta el final del día
    /// <see cref="ValidUntil"/> en hora de Costa Rica (RN-14).
    /// </summary>
    public bool IsWithinValidity(DateTimeOffset now) =>
        ValidUntil is { } validUntil && CostaRicaTime.ToLocalDate(now) <= validUntil;

    // ---------------------------------------------------------------
    // Auxiliares
    // ---------------------------------------------------------------

    private void Close(QuoteStatus status, DateTimeOffset now, string? reason)
    {
        Status = status;
        ClosedAt = now;
        CloseReason = reason;
    }

    private QuoteLine? FindLine(Guid productId) =>
        _lines.FirstOrDefault(l => l.ProductId == productId);

    private QuoteLine GetLine(Guid productId) =>
        FindLine(productId)
        ?? throw new DomainException("El producto no forma parte de la cotización.");

    private void EnsureDraft()
    {
        if (Status != QuoteStatus.Draft)
        {
            throw new DomainException("Solo se puede modificar una cotización en estado borrador.");
        }
    }

    private void EnsureStatus(QuoteStatus expected, string action)
    {
        if (Status != expected)
        {
            throw new DomainException(
                $"No se puede {action} una cotización en estado {Status}.");
        }
    }

    private void EnsureCurrentCustomer(Customer? customer)
    {
        if (customer is null)
        {
            throw new DomainException("El cliente es obligatorio.");
        }

        if (customer.Id != CustomerId)
        {
            throw new DomainException("El cliente no corresponde a la cotización.");
        }
    }

    private static void EnsureActiveCustomer(Customer? customer)
    {
        if (customer is null)
        {
            throw new DomainException("El cliente es obligatorio.");
        }

        if (!customer.IsActive)
        {
            throw new DomainException("No se puede usar un cliente inactivo en una cotización.");
        }
    }

    private static void EnsureValidNumber(string? number, int issueYear)
    {
        if (string.IsNullOrWhiteSpace(number) || !NumberPattern().IsMatch(number))
        {
            throw new DomainException("El número de cotización debe tener el formato COT-AAAA-000001.");
        }

        var year = int.Parse(number.AsSpan(4, 4), CultureInfo.InvariantCulture);

        if (year != issueYear)
        {
            throw new DomainException(
                "El año del número de cotización debe coincidir con el año de emisión.");
        }
    }

    [GeneratedRegex(@"^COT-[0-9]{4}-[0-9]{6}$", RegexOptions.CultureInvariant)]
    private static partial Regex NumberPattern();
}
