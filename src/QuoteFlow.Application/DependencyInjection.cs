using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Quotes.Common;

namespace QuoteFlow.Application;

public static class DependencyInjection
{
    private static readonly Type[] HandlerInterfaces =
    [
        typeof(ICommandHandler<>),
        typeof(ICommandHandler<,>),
        typeof(IQueryHandler<,>)
    ];

    /// <summary>
    /// Registra los servicios de la capa de aplicación. Los manejadores de comandos y consultas
    /// de este ensamblado se registran automáticamente con ciclo de vida <c>Scoped</c>.
    /// El reloj (<see cref="TimeProvider"/>) es inyectable para probar las reglas que dependen
    /// de la fecha; solo se registra el del sistema si no hay otro.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<QuoteDraftCalculator>();
        services.AddScoped<Documents.QuoteDocumentBuilder>();

        RegisterHandlers(services, typeof(DependencyInjection).Assembly);
        return services;
    }

    private static void RegisterHandlers(IServiceCollection services, Assembly assembly)
    {
        var implementations = assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false });

        foreach (var implementation in implementations)
        {
            var handlerInterfaces = implementation.GetInterfaces()
                .Where(i => i.IsGenericType && HandlerInterfaces.Contains(i.GetGenericTypeDefinition()));

            foreach (var service in handlerInterfaces)
            {
                services.AddScoped(service, implementation);
            }
        }
    }
}
