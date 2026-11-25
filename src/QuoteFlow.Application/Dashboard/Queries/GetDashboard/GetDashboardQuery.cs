using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Dashboard.Common;

namespace QuoteFlow.Application.Dashboard.Queries.GetDashboard;

/// <summary>
/// Dashboard (RF-DSH-01 a 03). Los indicadores y las cotizaciones recientes son globales para ambos
/// roles (Ventas ve todas las cotizaciones: RN-32); la actividad reciente es global para el
/// Administrador y solo la propia para Ventas (sección 10.2).
/// </summary>
public sealed record GetDashboardQuery : IQuery<DashboardDto>;
