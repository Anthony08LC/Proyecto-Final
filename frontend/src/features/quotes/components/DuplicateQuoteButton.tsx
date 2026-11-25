import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router'
import { getErrorMessage } from '../../../api/apiError'
import { duplicateQuote, type QuoteDetail } from '../../../api/quotes'
import { Button } from '../../../components/ui/Button'
import { useToast } from '../../../components/ui/toastContext'
import { quoteDisplayNumber } from '../../../lib/quoteStatus'
import { customerKeys } from '../../customers/queries'
import { quoteKeys } from '../queries'

/** Aviso que recibe el borrador nuevo al abrirse tras duplicar. */
export interface DuplicatedState {
  duplicatedFrom: string
  omittedProducts: string[]
}

/**
 * Duplica la cotización en un borrador nuevo con precios actuales (RF-COT-12, RN-23 a 25) y lo
 * abre. Las líneas con productos inactivos se omiten y se avisan en el borrador nuevo.
 */
export function DuplicateQuoteButton({ quote }: { quote: QuoteDetail }) {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const { notify } = useToast()

  const mutation = useMutation({
    mutationFn: () => duplicateQuote(quote.id),
    onSuccess: async (result) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: quoteKeys.all }),
        queryClient.invalidateQueries({ queryKey: customerKeys.all }),
      ])
      notify('Se creó un borrador nuevo a partir de la cotización.')
      const state: DuplicatedState = {
        duplicatedFrom: quoteDisplayNumber(quote.number),
        omittedProducts: result.omittedProducts,
      }
      navigate(`/quotes/${result.quoteId}`, { state })
    },
  })

  return (
    <>
      <Button onClick={() => mutation.mutate()} isLoading={mutation.isPending}>
        Duplicar
      </Button>
      <p className="muted">
        Crea un borrador nuevo con el mismo cliente, líneas y textos, a precios actuales. Esta
        cotización no cambia.
      </p>
      {mutation.error && (
        <p className="field__error" role="alert">
          {getErrorMessage(mutation.error)}
        </p>
      )}
    </>
  )
}
