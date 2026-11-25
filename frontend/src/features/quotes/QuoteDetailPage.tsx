import { useParams } from 'react-router'
import { getErrorMessage } from '../../api/apiError'
import type { QuoteDetail } from '../../api/quotes'
import { Alert, Spinner } from '../../components/ui/Feedback'
import { PageHeader } from '../../components/ui/PageHeader'
import { quoteDisplayNumber } from '../../lib/quoteStatus'
import { BreakdownPanel } from './components/BreakdownPanel'
import { QuoteHistory } from './components/QuoteHistory'
import { QuoteLinesTable } from './components/QuoteLinesTable'
import { QuoteStatusBadge } from './components/QuoteStatusBadge'
import { QuoteSummaryCard } from './components/QuoteSummaryCard'
import { QuoteTextsView } from './components/QuoteTextsView'
import { useQuote } from './queries'

/** Detalle de una cotización (RF-COT-13). */
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
  return <QuoteView quote={data} />
}

function QuoteView({ quote }: { quote: QuoteDetail }) {
  return (
    <>
      <PageHeader
        title={quoteDisplayNumber(quote.number)}
        description={<QuoteStatusBadge status={quote.status} />}
        back={{ to: '/quotes', label: 'Cotizaciones' }}
      />
      <div className="quote-layout">
        <div className="quote-layout__main">
          <QuoteSummaryCard quote={quote} />
          <QuoteLinesTable lines={quote.lines} />
          <QuoteTextsView quote={quote} />
          <QuoteHistory entries={quote.history} />
        </div>
        <aside className="quote-layout__side">
          <BreakdownPanel breakdown={quote.breakdown} />
        </aside>
      </div>
    </>
  )
}
