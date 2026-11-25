/**
 * Configuración del frontend leída de las variables de entorno de Vite (archivo `.env`, ver
 * `.env.example`). Todo lo que empieza con `VITE_` queda visible en el navegador: aquí nunca
 * van secretos (RNF-SEG-07).
 */

function readApiUrl(): string {
  const value = import.meta.env.VITE_API_URL

  if (typeof value !== 'string' || value.trim() === '') {
    throw new Error(
      'Falta la variable VITE_API_URL. Copie .env.example como .env e indique la URL de la API.',
    )
  }

  // Sin barra final, para componer rutas como `${apiUrl}/api/...`.
  return value.trim().replace(/\/+$/, '')
}

export const env = {
  apiUrl: readApiUrl(),
} as const
