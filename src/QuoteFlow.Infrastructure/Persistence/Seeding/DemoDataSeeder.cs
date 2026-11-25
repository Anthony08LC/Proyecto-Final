using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Domain.Common;
using QuoteFlow.Domain.Entities;
using QuoteFlow.Domain.Enums;
using QuoteFlow.Domain.ValueObjects;
using QuoteFlow.Infrastructure.Persistence.Records;

namespace QuoteFlow.Infrastructure.Persistence.Seeding;

/// <summary>
/// Datos de demostración (sección 8.3 del plan, T-090): 6 clientes (2 VIP), 8 productos en las
/// cuatro categorías y cotizaciones en todos los estados, con casos para cada tramo de descuento,
/// incluido el caso C-09 (₡1.200.000,00 para un cliente VIP → total ₡1.179.720,00).
/// <para>
/// Solo se ejecuta en el entorno de desarrollo: lo verifica explícitamente y se niega en cualquier otro.
/// Es idempotente: los clientes se identifican por correo y los productos por nombre, y las
/// cotizaciones solo se crean si ningún cliente de demostración tiene cotizaciones.
/// </para>
/// Requiere los datos iniciales de <see cref="DatabaseSeeder"/> (categorías, configuración y un Administrador,
/// que queda como propietario de las cotizaciones).
/// </summary>
public sealed class DemoDataSeeder(
    ApplicationDbContext dbContext,
    IQuoteNumberGenerator numberGenerator,
    IHostEnvironment environment,
    TimeProvider timeProvider,
    ILogger<DemoDataSeeder> logger)
{
    // Tipos de evento de las cotizaciones (los mismos de QuoteAuditEvents en la capa de aplicación).
    private const string QuoteCreatedEvent = "QuoteCreated";
    private const string QuoteIssuedEvent = "QuoteIssued";
    private const string QuoteAcceptedEvent = "QuoteAccepted";
    private const string QuoteRejectedEvent = "QuoteRejected";
    private const string QuoteCancelledEvent = "QuoteCancelled";
    private const string QuoteExpiredEvent = "QuoteExpired";

    private static readonly NumberFormatInfo CostaRicaNumbers = new()
    {
        NumberDecimalSeparator = ",",
        NumberGroupSeparator = ".",
        NumberGroupSizes = [3]
    };

    private static readonly DemoCustomer[] Customers =
    [
        new("Laura Jiménez Mora", "Distribuidora Central del Valle S.A.", "laura.jimenez@centraldelvalle.example", "2222-3344", IsVip: true),
        new("Carlos Vargas Rojas", "Clínica Santa Lucía", "carlos.vargas@clinicasantalucia.example", "8811-2233", IsVip: true),
        new("Andrea Castro Quesada", "Constructora Horizonte", "andrea.castro@horizonte.example", "2290-1020", IsVip: false),
        new("José Pablo Araya Solano", "Café Montaña Azul", "jpablo.araya@montanaazul.example", "8765-4321", IsVip: false),
        new("Daniela Méndez Ulate", "Academia Lingua", "daniela.mendez@lingua.example", "2441-5566", IsVip: false),
        new("Roberto Chaves Brenes", null, "roberto.chaves@correo.example", "7012-3456", IsVip: false)
    ];

    private static readonly DemoProduct[] Products =
    [
        new("Laptop empresarial 14\"", "Procesador de 8 núcleos, 16 GB de RAM, SSD de 512 GB.", "Equipo tecnológico", 650_000m),
        new("Monitor 27\" 4K", "Panel IPS con ajuste de altura y puertos USB-C.", "Equipo tecnológico", 185_000m),
        new("Suite ofimática (licencia anual)", "Procesador de texto, hoja de cálculo y correo por usuario.", "Licencia de software", 95_000m),
        new("Antivirus corporativo (licencia anual)", "Protección de equipo administrada desde consola central.", "Licencia de software", 28_500m),
        new("Hora de consultoría técnica", "Análisis, diseño o acompañamiento técnico por hora.", "Servicio profesional", 35_000m),
        new("Instalación y configuración de red", "Cableado, configuración de switch, firewall y Wi-Fi para oficina pequeña.", "Servicio profesional", 250_000m),
        new("Servidor de archivos con respaldo", "Servidor con almacenamiento redundante y respaldo diario en la nube.", "Solución empresarial", 1_200_000m),
        new("Sistema de punto de venta", "Software, terminal e impresora de tiquetes para un punto de venta.", "Solución empresarial", 480_000m)
    ];

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        // Verificación explícita: los datos de demostración nunca se cargan fuera de desarrollo.
        if (!environment.IsDevelopment())
        {
            throw new InvalidOperationException(
                $"Los datos de demostración solo se cargan en Development (entorno actual: {environment.EnvironmentName}).");
        }

        var now = timeProvider.GetUtcNow();
        var systemUser = await dbContext.Users.SingleOrDefaultAsync(u => u.IsSystem, cancellationToken);
        var settings = await dbContext.SystemSettings.SingleOrDefaultAsync(cancellationToken);

        if (systemUser is null || settings is null)
        {
            logger.LogWarning("Faltan los datos iniciales (usuario de sistema o configuración): no se cargaron los datos de demostración.");
            return;
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var customers = await EnsureCustomersAsync(systemUser.Id, now, cancellationToken);
        var (products, categories) = await EnsureProductsAsync(systemUser.Id, now, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        await EnsureQuotesAsync(customers, products, categories, settings, systemUser, now, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    // ---------------------------------------------------------------
    // Clientes y productos
    // ---------------------------------------------------------------

    private async Task<Dictionary<string, Customer>> EnsureCustomersAsync(
        Guid systemUserId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var byEmail = new Dictionary<string, Customer>(StringComparer.OrdinalIgnoreCase);
        var created = 0;

        foreach (var demo in Customers)
        {
            var email = Email.Create(demo.Email);
            var existing = await dbContext.Customers.FirstOrDefaultAsync(c => c.Email == email, cancellationToken);

            if (existing is not null)
            {
                byEmail[demo.Email] = existing;
                continue;
            }

            var customer = Customer.Create(
                demo.Name,
                demo.Company,
                email,
                Phone.Create(demo.Phone),
                notes: "Cliente de demostración.",
                demo.IsVip);

            customer.MarkCreated(systemUserId, now);
            dbContext.Customers.Add(customer);
            byEmail[demo.Email] = customer;
            created++;
        }

        logger.LogInformation("Datos de demostración: {Created} clientes creados.", created);
        return byEmail;
    }

    private async Task<(Dictionary<string, Product> Products, List<Category> Categories)> EnsureProductsAsync(
        Guid systemUserId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var categories = await dbContext.Categories.ToListAsync(cancellationToken);
        var categoriesByName = categories.ToDictionary(c => c.Name, StringComparer.OrdinalIgnoreCase);

        var names = Products.Select(p => p.Name.ToLower()).ToList();
        var existing = await dbContext.Products
            .Where(p => names.Contains(p.Name.ToLower()))
            .ToListAsync(cancellationToken);

        var byName = existing.ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
        var created = 0;

        foreach (var demo in Products)
        {
            if (byName.ContainsKey(demo.Name))
            {
                continue;
            }

            if (!categoriesByName.TryGetValue(demo.Category, out var category) || !category.IsActive)
            {
                logger.LogWarning(
                    "Datos de demostración: la categoría «{Category}» no existe o está inactiva; se omite «{Product}».",
                    demo.Category,
                    demo.Name);
                continue;
            }

            var product = Product.Create(demo.Name, demo.Description, category, Money.Create(demo.Price));
            product.MarkCreated(systemUserId, now);
            dbContext.Products.Add(product);
            byName[demo.Name] = product;
            created++;
        }

        logger.LogInformation("Datos de demostración: {Created} productos creados.", created);
        return (byName, categories);
    }

    // ---------------------------------------------------------------
    // Cotizaciones
    // ---------------------------------------------------------------

    private async Task EnsureQuotesAsync(
        Dictionary<string, Customer> customers,
        Dictionary<string, Product> products,
        List<Category> categories,
        SystemSettings settings,
        User systemUser,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var customerIds = customers.Values.Select(c => c.Id).ToList();

        if (await dbContext.Quotes.AnyAsync(q => customerIds.Contains(q.CustomerId), cancellationToken))
        {
            logger.LogInformation("Datos de demostración: las cotizaciones ya existen.");
            return;
        }

        var owner = await dbContext.Users
            .Where(u => !u.IsSystem && u.IsActive && u.Role == UserRole.Administrator)
            .OrderBy(u => u.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (owner is null)
        {
            logger.LogWarning("Datos de demostración: no hay un Administrador activo; no se crearon cotizaciones.");
            return;
        }

        if (Products.Any(p => !products.ContainsKey(p.Name)) || customers.Values.Any(c => !c.IsActive))
        {
            logger.LogWarning("Datos de demostración: faltan productos o hay clientes inactivos; no se crearon cotizaciones.");
            return;
        }

        var context = new QuoteContext(owner, systemUser, products, categories, settings);
        var validity = settings.ValidityDays;

        // Ordenadas por fecha de emisión, para que los consecutivos sigan el orden cronológico.
        // Las fechas se calculan con la vigencia configurada, para que cada transición sea válida.
        var scenarios = new DemoQuote[]
        {
            // Vencida: emitida hace (vigencia + 5) días, vencida el día siguiente a su vigencia. Sin tramo.
            new(customers["daniela.mendez@lingua.example"], [("Suite ofimática (licencia anual)", 5m)],
                CreatedAt: now.AddDays(-(validity + 6)), IssuedAt: now.AddDays(-(validity + 5)),
                Close: (QuoteStatus.Expired, now.AddDays(-4), null)),

            // Aceptada: tramo 2 (10 %) para un cliente no VIP; subtotal ₡1.300.000,00.
            new(customers["andrea.castro@horizonte.example"], [("Laptop empresarial 14\"", 2m)],
                CreatedAt: now.AddDays(-9), IssuedAt: now.AddDays(-8),
                Close: (QuoteStatus.Accepted, now.AddDays(-7), null)),

            // Rechazada con motivo: tramo 1 (5 %); subtotal ₡730.000,00.
            new(customers["jpablo.araya@montanaazul.example"],
                [("Sistema de punto de venta", 1m), ("Instalación y configuración de red", 1m)],
                CreatedAt: now.AddDays(-7), IssuedAt: now.AddDays(-6),
                Close: (QuoteStatus.Rejected, now.AddDays(-4), "El cliente pospuso la inversión para el próximo año.")),

            // Cancelada después de emitir (motivo obligatorio): tramo 1 + VIP (8 %); subtotal ₡897.500,00.
            new(customers["carlos.vargas@clinicasantalucia.example"],
                [("Monitor 27\" 4K", 4m), ("Hora de consultoría técnica", 4.5m)],
                CreatedAt: now.AddDays(-6), IssuedAt: now.AddDays(-5),
                Close: (QuoteStatus.Cancelled, now.AddDays(-4), "Se emitió con cantidades equivocadas; se reemplaza por otra cotización.")),

            // Emitida: tramo 1 (5 %); subtotal ₡835.000,00.
            new(customers["andrea.castro@horizonte.example"],
                [("Laptop empresarial 14\"", 1m), ("Monitor 27\" 4K", 1m)],
                CreatedAt: now.AddDays(-3), IssuedAt: now.AddDays(-2), Close: null),

            // Emitida: caso C-09, cliente VIP con subtotal ₡1.200.000,00 → 13 %, total ₡1.179.720,00.
            new(customers["laura.jimenez@centraldelvalle.example"], [("Servidor de archivos con respaldo", 1m)],
                CreatedAt: now.AddDays(-2), IssuedAt: now.AddDays(-1), Close: null),

            // Borrador: subtotal exactamente ₡500.000,00, en el umbral 1 → sin descuento por tramo.
            new(customers["andrea.castro@horizonte.example"], [("Instalación y configuración de red", 2m)],
                CreatedAt: now.AddDays(-1), IssuedAt: null, Close: null),

            // Borrador: solo descuento VIP (3 %); subtotal ₡285.000,00.
            new(customers["carlos.vargas@clinicasantalucia.example"], [("Antivirus corporativo (licencia anual)", 10m)],
                CreatedAt: now.AddHours(-5), IssuedAt: null, Close: null),

            // Cancelada desde borrador (motivo opcional, sin motivo).
            new(customers["jpablo.araya@montanaazul.example"], [("Hora de consultoría técnica", 8m)],
                CreatedAt: now.AddDays(-3), IssuedAt: null,
                Close: (QuoteStatus.Cancelled, now.AddDays(-2), null))
        };

        foreach (var scenario in scenarios)
        {
            await CreateQuoteAsync(scenario, context, cancellationToken);
        }

        logger.LogInformation("Datos de demostración: {Created} cotizaciones creadas.", scenarios.Length);
    }

    private async Task CreateQuoteAsync(DemoQuote demo, QuoteContext context, CancellationToken cancellationToken)
    {
        var customer = demo.Customer;
        var quote = Quote.Create(customer, context.Owner.Id, context.Settings.DefaultCommercialTerms);

        quote.UpdateTexts(
            introduction: $"Estimado(a) {customer.Name}: le presentamos nuestra propuesta.",
            executiveSummary: "Propuesta de demostración generada automáticamente.",
            notes: null,
            commercialTerms: context.Settings.DefaultCommercialTerms);

        foreach (var (productName, quantity) in demo.Lines)
        {
            var product = context.Products[productName];
            quote.AddLine(product, context.CategoryOf(product), quantity);
        }

        quote.Recalculate(customer, context.Settings);
        quote.MarkCreated(context.Owner.Id, demo.CreatedAt);
        AddEvent(quote, QuoteCreatedEvent, demo.CreatedAt, context, $"Borrador creado para el cliente «{customer.Name}».");

        if (demo.IssuedAt is { } issuedAt)
        {
            var number = await numberGenerator.NextAsync(CostaRicaTime.ToLocalDate(issuedAt).Year, cancellationToken);
            var lineProducts = demo.Lines.Select(l => context.Products[l.Product]).Distinct().ToList();

            quote.Emit(number, customer, lineProducts, context.Categories, context.Settings, issuedAt);
            quote.MarkUpdated(context.Owner.Id, issuedAt);
            AddEvent(quote, QuoteIssuedEvent, issuedAt, context,
                $"Cotización {number} emitida para «{customer.Name}» por un total de "
                + $"{Colones(quote.Breakdown.Total.Amount)}; vigente hasta {Date(quote.ValidUntil!.Value)}.");
        }

        if (demo.Close is { } close)
        {
            var (status, closedAt, reason) = close;
            var label = quote.Number ?? "(borrador)";
            var suffix = reason is null ? string.Empty : $" Motivo: {reason}";

            switch (status)
            {
                case QuoteStatus.Accepted:
                    quote.Accept(closedAt);
                    AddEvent(quote, QuoteAcceptedEvent, closedAt, context, $"Cotización {label} aceptada por el cliente.");
                    break;
                case QuoteStatus.Rejected:
                    quote.Reject(closedAt, reason);
                    AddEvent(quote, QuoteRejectedEvent, closedAt, context, $"Cotización {label} rechazada por el cliente.{suffix}");
                    break;
                case QuoteStatus.Cancelled:
                    quote.Cancel(closedAt, reason);
                    AddEvent(quote, QuoteCancelledEvent, closedAt, context, $"Cotización {label} cancelada.{suffix}");
                    break;
                case QuoteStatus.Expired:
                    quote.Expire(closedAt);
                    AddEvent(quote, QuoteExpiredEvent, closedAt, context,
                        $"Cotización {label} vencida (vigente hasta {Date(quote.ValidUntil!.Value)}).",
                        automatic: true);
                    break;
                default:
                    throw new InvalidOperationException($"Estado de cierre no soportado: {status}.");
            }

            // El vencimiento lo hace el proceso automático, a nombre del usuario de sistema.
            var closedBy = status == QuoteStatus.Expired ? context.SystemUser : context.Owner;
            quote.MarkUpdated(closedBy.Id, closedAt);
        }

        dbContext.Quotes.Add(quote);
    }

    private void AddEvent(
        Quote quote,
        string eventType,
        DateTimeOffset occurredAt,
        QuoteContext context,
        string detail,
        bool automatic = false)
    {
        var actor = automatic ? context.SystemUser : context.Owner;

        dbContext.AuditEvents.Add(new AuditEvent
        {
            OccurredAt = occurredAt,
            UserId = actor.Id,
            UserName = actor.FullName,
            EventType = eventType,
            EntityType = nameof(AuditEntityType.Quote),
            EntityId = quote.Id,
            Detail = detail
        });
    }

    private static string Colones(decimal amount) => "₡" + amount.ToString("#,##0.00", CostaRicaNumbers);

    private static string Date(DateOnly date) => date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

    private sealed record DemoCustomer(string Name, string? Company, string Email, string Phone, bool IsVip);

    private sealed record DemoProduct(string Name, string Description, string Category, decimal Price);

    private sealed record DemoQuote(
        Customer Customer,
        (string Product, decimal Quantity)[] Lines,
        DateTimeOffset CreatedAt,
        DateTimeOffset? IssuedAt,
        (QuoteStatus Status, DateTimeOffset ClosedAt, string? Reason)? Close);

    private sealed class QuoteContext(
        User owner,
        User systemUser,
        Dictionary<string, Product> products,
        List<Category> categories,
        SystemSettings settings)
    {
        public User Owner { get; } = owner;

        public User SystemUser { get; } = systemUser;

        public Dictionary<string, Product> Products { get; } = products;

        public List<Category> Categories { get; } = categories;

        public SystemSettings Settings { get; } = settings;

        public Category CategoryOf(Product product) => Categories.Single(c => c.Id == product.CategoryId);
    }
}
