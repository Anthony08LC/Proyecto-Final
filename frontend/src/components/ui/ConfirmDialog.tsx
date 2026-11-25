import { useEffect, useRef, type ReactNode } from 'react'
import { Button } from './Button'

interface ConfirmDialogProps {
  open: boolean
  title: string
  /** Consecuencias de la acción, explicadas al usuario (RNF-USA-06). */
  children: ReactNode
  confirmLabel: string
  tone?: 'primary' | 'danger'
  isLoading?: boolean
  error?: string | null
  onConfirm: () => void
  onCancel: () => void
}

/**
 * Confirmación de acciones destructivas o de impacto (RNF-USA-06). Usa `<dialog>` nativo: atrapa
 * el foco, se cierra con Escape y es accesible por teclado.
 */
export function ConfirmDialog({
  open,
  title,
  children,
  confirmLabel,
  tone = 'primary',
  isLoading = false,
  error,
  onConfirm,
  onCancel,
}: ConfirmDialogProps) {
  const ref = useRef<HTMLDialogElement>(null)

  useEffect(() => {
    const dialog = ref.current
    if (!dialog) {
      return
    }
    if (open && !dialog.open) {
      dialog.showModal()
    } else if (!open && dialog.open) {
      dialog.close()
    }
  }, [open])

  return (
    <dialog
      ref={ref}
      className="dialog"
      aria-labelledby="confirm-title"
      onCancel={(event) => {
        event.preventDefault()
        if (!isLoading) {
          onCancel()
        }
      }}
    >
      <h2 id="confirm-title" className="dialog__title">
        {title}
      </h2>
      <div className="dialog__body">{children}</div>
      {error && (
        <p className="field__error" role="alert">
          {error}
        </p>
      )}
      <div className="dialog__actions">
        <Button onClick={onCancel} disabled={isLoading}>
          Volver
        </Button>
        <Button variant={tone} onClick={onConfirm} isLoading={isLoading} autoFocus>
          {confirmLabel}
        </Button>
      </div>
    </dialog>
  )
}
