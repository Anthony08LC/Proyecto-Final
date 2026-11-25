import { useMutation, useQuery } from '@tanstack/react-query'
import { getErrorMessage } from '../../../api/apiError'
import { getQuotePdf, getQuotePreview, type QuoteDetail } from '../../../api/quotes'
import { Button } from '../../../components/ui/Button'
import { Alert, Spinner } from '../../../components/ui/Feedback'
import { useToast } from '../../../components/ui/toastContext'
import { quoteKeys } from '../queries'

/**
 * Vista previa HTML del documento comercial (RF-DOC-01), dentro de la página. Se muestra en un
 * marco aislado (`sandbox` vacío: sin scripts ni acceso a la aplicación). La consulta cuelga de la
 * clave del detalle, así que se actualiza tras cada cambio del borrador.
 */
export function QuoteDocumentPreview({
  quote,
  onClose,
}: {
  quote: QuoteDetail
  onClose: () => void
}) {
  const { data, error, isPending, isFetching } = useQuery({
    queryKey: [...quoteKeys.detail(quote.id), 'preview'],
    queryFn: ({ signal }) => getQuotePreview(quote.id, signal),
  })

  return (
    <section className="card document-preview" aria-labelledby="preview-title">
      <div className="card__header">
        <h2 id="preview-title" className="card__title">
          Vista previa del documento
        </h2>
        <div className="document-preview__actions">
          {isFetching && !isPending && <span className="muted">Actualizando…</span>}
          <Button onClick={onClose}>Cerrar vista previa</Button>
        </div>
      </div>
      {error && <Alert tone="danger">{getErrorMessage(error)}</Alert>}
      {isPending && <Spinner label="Generando vista previa…" />}
      {data && (
        <iframe
          className="document-preview__frame"
          title={`Vista previa de ${quote.number ?? 'la cotización en borrador'}`}
          sandbox=""
          srcDoc={data}
        />
      )}
    </section>
  )
}

/** Descarga el PDF de una cotización emitida como COT-AAAA-000001.pdf (RF-DOC-02, RN-36). */
export function DownloadPdfButton({ quote }: { quote: QuoteDetail }) {
  const { notify } = useToast()

  const download = useMutation({
    mutationFn: () => getQuotePdf(quote.id),
    onSuccess: (pdf) => {
      const url = URL.createObjectURL(pdf)
      const link = document.createElement('a')
      link.href = url
      link.download = `${quote.number ?? 'cotizacion'}.pdf`
      link.click()
      // Se libera después de que el navegador inicia la descarga.
      window.setTimeout(() => URL.revokeObjectURL(url), 10_000)
    },
    onError: (error) => notify(getErrorMessage(error), 'danger'),
  })

  return (
    <Button onClick={() => download.mutate()} isLoading={download.isPending}>
      Descargar PDF
    </Button>
  )
}
