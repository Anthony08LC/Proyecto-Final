using System.Text.Json.Serialization;
using QuoteFlow.Api.BackgroundJobs;
using QuoteFlow.Api.ErrorHandling;
using QuoteFlow.Api.Health;
using QuoteFlow.Api.OpenApi;
using QuoteFlow.Api.Security;
using QuoteFlow.Application;
using QuoteFlow.Infrastructure;

const string FrontendCorsPolicy = "Frontend";

var builder = WebApplication.CreateBuilder(args);

// Capas de la aplicación.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// API: controladores, JSON con enumeraciones como texto y cambio obligatorio de contraseña (RF-AUT-07).
builder.Services
    .AddControllers(options => options.Filters.Add<PasswordChangeRequiredFilter>())
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        // No exponer los mensajes internos del parser JSON (RNF-SEG-10).
        options.AllowInputFormatterExceptionMessages = false;
    });
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Transversales: errores, seguridad, documentación y salud.
builder.Services.AddApiProblemDetails();
builder.Services.AddJwtAuthentication();
builder.Services.AddApiRateLimiting();
builder.Services.AddApiDocumentation();
builder.Services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("database");

// CORS: solo el origen del frontend (RNF-SEG-09).
builder.Services.AddCors(options => options.AddPolicy(FrontendCorsPolicy, policy => policy
    .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
    .AllowAnyHeader()
    .AllowAnyMethod()));

// Vencimiento automático de cotizaciones (RF-COT-11).
builder.Services.Configure<QuoteExpirationOptions>(
    builder.Configuration.GetSection(QuoteExpirationOptions.SectionName));
builder.Services.AddHostedService<QuoteExpirationService>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseApiDocumentation();
}
else
{
    // HTTPS obligatorio en producción (RNF-SEG-01).
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseCors(FrontendCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapHealthChecks("/health").AllowAnonymous();
app.MapControllers();

// Datos iniciales (sección 9.7): requiere la base de datos con las migraciones aplicadas.
if (app.Configuration.GetValue<bool>("Database:SeedOnStartup"))
{
    await app.Services.SeedDatabaseAsync();

    // Datos de demostración (T-090): nunca fuera de desarrollo; el seeder también lo verifica.
    if (app.Environment.IsDevelopment() && app.Configuration.GetValue<bool>("Database:SeedDemoData"))
    {
        await app.Services.SeedDemoDataAsync();
    }
}

await app.RunAsync();

/// <summary>Punto de entrada; parcial para permitir pruebas de integración con WebApplicationFactory.</summary>
public partial class Program;
