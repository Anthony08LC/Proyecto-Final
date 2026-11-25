import { useSession } from '../auth/useSession'
import { PageHeader } from '../components/ui/PageHeader'
import { HealthStatusCard } from '../features/health/HealthStatusCard'

/** Inicio provisional: el dashboard (RF-DSH) requiere endpoints que la API aún no tiene. */
export function HomePage() {
  const { user } = useSession()

  return (
    <>
      <PageHeader
        title={`Hola, ${user?.fullName ?? ''}`}
        description="TechSolutions CR · Gestión de cotizaciones"
      />
      <HealthStatusCard />
    </>
  )
}
