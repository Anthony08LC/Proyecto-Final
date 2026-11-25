import type { QuoteStatus } from '../../../api/types'
import { Badge } from '../../../components/ui/Feedback'
import { QUOTE_STATUS_LABELS, QUOTE_STATUS_TONES } from '../../../lib/quoteStatus'

export function QuoteStatusBadge({ status }: { status: QuoteStatus }) {
  return <Badge tone={QUOTE_STATUS_TONES[status]}>{QUOTE_STATUS_LABELS[status]}</Badge>
}
