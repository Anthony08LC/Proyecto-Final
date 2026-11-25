import { useState, type ReactNode } from 'react'
import { useLocation, useNavigate, useParams } from 'react-router'
import { getErrorMessage } from '../../api/apiError'
import type { QuoteDetail } from '../../api/quotes'
import { Button } from '../../components/ui/Button'
import { Alert, Spinner } from '../../components/ui/Feedback'
import { PageHeader } from '../../components/ui/PageHeader'
import { quoteDisplayNumber } from '../../lib/quoteStatus'
import { BreakdownPanel } from './components/BreakdownPanel'
import { DuplicateQuoteButton, type DuplicatedState } from './components/DuplicateQuoteButton'
import { QuoteActionsPanel } from './components/QuoteActionsPanel'
import { QuoteHistory } from './components/QuoteHistory'
import { QuoteLinesTable } from './components/QuoteLinesTable'
import { QuoteStatusBadge } from './components/QuoteStatusBadge'
import { QuoteSummaryCard } from './components/QuoteSummaryCard'
import { QuoteTextsView } from './components/QuoteTextsView'
import { QuoteEditor } from './editor/QuoteEditor'
import { useQuote } from './queries'

/**
 * Detalle de una cotización (RF-COT-13). Si es un borrador que el usuario puede modificar (según
 * `actions.canEdit` de la API, RN-32), se muestra el editor.
 */
export function QuoteDetailPage() {
  const { id = '' } = useParams()
  const { data, error, isPending } = useQuote(id)

  if (isPending) {
    return <Spinner />
  }
  if (error) {
    return (
      <>
        <PageHeader title="Cotización" back={{ to: '/quotes', label: 'Cotizaciones' }} />
        <Alert tone="danger">{getErrorMessage(error)}</Alert>
      </>
    )
  }
  // La clave reinicia el editor (formularios y estado) al pasar de una cotización a otra,
  // por ejemplo al duplicar o al seguir el enlace a la cotización de origen.
  return <QuoteView key={data.id} quote={data} />
}

function QuoteView({ quote }: { quote: QuoteDetail }) {
  const [textsDirty, setTextsDirty] = useState(false)
  const isEditable = quote.status === 'Draft' && quote.actions.canEdit
  const duplicate = quote.actions.canDuplicate ? <DuplicateQuoteButton quote={quote} /> : null

  return (
    <>
      <PageHeader
        title={quoteDisplayNumber(quote.number)}
        description={<QuoteStatusBadge status={quote.status} />}
        back={{ to: '/quotes', label: 'Cotizaciones' }}
      />
      <DuplicatedNotice />
      {isEditable ? (
        <QuoteEditor
          quote={quote}
          actions={
            <QuoteActionsPanel quote={quote} hasUnsavedTexts={textsDirty}>
              {duplicate}
            </QuoteActionsPanel>
          }
          onTextsDirtyChange={setTextsDirty}
        />
      ) : (
        <QuoteReadOnly quote={quote} duplicate={duplicate} />
      )}
    </>
  )
}

/** Cotización emitida o cerrada, o borrador ajeno (Ventas solo modifica los suyos, RN-32). */
function QuoteReadOnly({ quote, duplicate }: { quote: QuoteDetail; duplicate: ReactNode }) {
  return (
    <>
      {quote.status === 'Draft' && (
        <Alert tone="neutral">
          Borrador de {quote.ownerName}. Solo su propietario o un Administrador pueden modificarlo.
        </Alert>
      )}
      <div className="quote-layout">
        <div className="quote-layout__main">
          <QuoteSummaryCard quote={quote} />
          <QuoteLinesTable lines={quote.lines} />
          <QuoteTextsView quote={quote} />
          <QuoteHistory entries={quote.history} />
        </div>
        <aside className="quote-layout__side">
          <BreakdownPanel breakdown={quote.breakdown} />
          <QuoteActionsPanel quote={quote}>{duplicate}</QuoteActionsPanel>
        </aside>
      </div>
    </>
  )
}

/**
 * Aviso del borrador recién duplicado (RF-COT-12): indica el origen y las líneas omitidas por
 * tener productos inactivos (RN-25). Viene en el estado de la navegación; al cerrarlo se limpia.
 */
function DuplicatedNotice() {
  const location = useLocation()
  const navigate = useNavigate()
  const state = location.state as DuplicatedState | null

  if (!state?.duplicatedFrom) {
    return null
  }

  const dismiss = () => navigate(location.pathname, { replace: true, state: null })
  const omitted = state.omittedProducts

  return (
    <Alert
      tone={omitted.length > 0 ? 'warning' : 'success'}
      title={`Borrador creado a partir de ${state.duplicatedFrom}`}
      actions={<Button onClick={dismiss}>Entendido</Button>}
    >
      {omitted.length > 0 ? (
        <>
          <p>Estas líneas no se copiaron porque sus productos están inactivos:</p>
          <ul className="omitted-list">
            {omitted.map((name) => (
              <li key={name}>{name}</li>
            ))}
          </ul>
        </>
      ) : (
        <p>Se copiaron todas las líneas con los precios actuales del catálogo.</p>
      )}
    </Alert>
  )
}
