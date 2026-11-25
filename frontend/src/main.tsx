import { QueryClientProvider } from '@tanstack/react-query'
import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { App } from './App'
import { queryClient } from './api/queryClient'
import { SessionProvider } from './auth/SessionProvider'
import { ToastProvider } from './components/ui/Toast'
import { applyStoredTheme } from './theme/theme'
import './index.css'

const root = document.getElementById('root')

if (!root) {
  throw new Error('No se encontró el elemento #root en index.html.')
}

applyStoredTheme()

createRoot(root).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <ToastProvider>
        <SessionProvider>
          <App />
        </SessionProvider>
      </ToastProvider>
    </QueryClientProvider>
  </StrictMode>,
)
