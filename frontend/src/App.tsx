import { HealthStatusCard } from './features/health/HealthStatusCard'

/**
 * Pantalla provisional del incremento 1: muestra el estado del backend. El layout, las rutas y la
 * sesión llegan en los siguientes incrementos.
 */
export function App() {
  return (
    <main className="page">
      <header className="page__header">
        <h1 className="page__title">QuoteFlow AI</h1>
        <p className="muted">TechSolutions CR · Gestión de cotizaciones</p>
      </header>
      <HealthStatusCard />
    </main>
  )
}
