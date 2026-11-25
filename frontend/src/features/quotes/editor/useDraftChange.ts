import { useIsMutating, useMutation, useQueryClient } from '@tanstack/react-query'
import type { QuoteBreakdown, QuoteDetail } from '../../../api/quotes'
import { quoteKeys } from '../queries'

const draftMutationKey = (quoteId: string) => ['quotes', 'draft', quoteId] as const

/**
 * Aplica un cambio al borrador (líneas, cliente o textos). Cuando la API devuelve el desglose
 * recalculado se muestra de inmediato (RF-COT-06); después se recarga el detalle para traer las
 * líneas renumeradas y los datos vigentes. El frontend nunca calcula los montos.
 *
 * Los cambios de un mismo borrador se ejecutan en serie (`scope`): dos solicitudes simultáneas
 * sobre la misma cotización chocarían con el control de concurrencia optimista (409).
 */
export function useDraftChange(quoteId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationKey: draftMutationKey(quoteId),
    scope: { id: `quote-draft-${quoteId}` },
    mutationFn: (change: () => Promise<QuoteBreakdown | void>) => change(),
    onSuccess: (breakdown) => {
      if (breakdown) {
        queryClient.setQueryData<QuoteDetail>(quoteKeys.detail(quoteId), (quote) =>
          quote ? { ...quote, breakdown } : quote,
        )
      }
    },
    // También tras un error: así la pantalla vuelve a mostrar el estado real del borrador.
    onSettled: () =>
      Promise.all([
        queryClient.invalidateQueries({ queryKey: quoteKeys.detail(quoteId) }),
        queryClient.invalidateQueries({ queryKey: [...quoteKeys.all, 'list'] }),
      ]),
  })
}

/** Verdadero mientras haya cambios del borrador en curso (para indicar que el desglose se actualiza). */
export function useIsDraftChanging(quoteId: string): boolean {
  return useIsMutating({ mutationKey: draftMutationKey(quoteId) }) > 0
}
