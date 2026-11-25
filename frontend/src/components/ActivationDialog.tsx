import { useMutation, useQueryClient, type QueryKey } from '@tanstack/react-query'
import { useState, type ReactNode } from 'react'
import { getErrorMessage } from '../api/apiError'
import { ConfirmDialog } from './ui/ConfirmDialog'
import { useToast } from './ui/toastContext'

/** Registro que se puede desactivar y reactivar (borrado lógico, spec 9.6). */
export interface Activatable {
  id: string
  name: string
  isActive: boolean
}

interface Options {
  /** Nombre del tipo de registro en minúsculas, por ejemplo «la categoría». */
  entityLabel: string
  deactivate: (id: string) => Promise<void>
  reactivate: (id: string) => Promise<void>
  /** Consultas que deben recargarse después del cambio. */
  invalidate: QueryKey
  /** Consecuencias de desactivar, explicadas antes de confirmar (RNF-USA-06). */
  deactivateConsequences: ReactNode
  reactivateConsequences: ReactNode
}

/**
 * Flujo común de desactivar y reactivar con confirmación (RNF-USA-06), compartido por
 * categorías, productos y clientes. Devuelve la función para pedir el cambio y el diálogo.
 */
export function useActivationDialog({
  entityLabel,
  deactivate,
  reactivate,
  invalidate,
  deactivateConsequences,
  reactivateConsequences,
}: Options) {
  const queryClient = useQueryClient()
  const { notify } = useToast()
  const [target, setTarget] = useState<Activatable | null>(null)

  const mutation = useMutation({
    mutationFn: (item: Activatable) => (item.isActive ? deactivate(item.id) : reactivate(item.id)),
    onSuccess: async (_, item) => {
      await queryClient.invalidateQueries({ queryKey: invalidate })
      notify(`Se ${item.isActive ? 'desactivó' : 'reactivó'} ${entityLabel} «${item.name}».`)
      setTarget(null)
    },
  })

  const request = (item: Activatable) => {
    mutation.reset()
    setTarget(item)
  }

  const deactivating = target?.isActive ?? false

  const dialog = (
    <ConfirmDialog
      open={target !== null}
      title={
        deactivating
          ? `¿Desactivar ${entityLabel} «${target?.name}»?`
          : `¿Reactivar ${entityLabel} «${target?.name}»?`
      }
      confirmLabel={deactivating ? 'Desactivar' : 'Reactivar'}
      tone={deactivating ? 'danger' : 'primary'}
      isLoading={mutation.isPending}
      error={mutation.error ? getErrorMessage(mutation.error) : null}
      onConfirm={() => target && mutation.mutate(target)}
      onCancel={() => setTarget(null)}
    >
      {deactivating ? deactivateConsequences : reactivateConsequences}
    </ConfirmDialog>
  )

  return { request, dialog }
}
