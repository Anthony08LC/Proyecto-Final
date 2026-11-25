import {
  forwardRef,
  useId,
  type InputHTMLAttributes,
  type ReactNode,
  type SelectHTMLAttributes,
  type TextareaHTMLAttributes,
} from 'react'

/**
 * Campos de formulario con etiqueta, ayuda y error junto al campo (RNF-USA-04). Usan
 * `forwardRef` para registrarse con React Hook Form.
 */

interface FieldShellProps {
  id: string
  label: string
  error?: string | undefined
  hint?: ReactNode
  required?: boolean | undefined
  children: ReactNode
}

function FieldShell({ id, label, error, hint, required, children }: FieldShellProps) {
  return (
    <div className={`field${error ? ' field--invalid' : ''}`}>
      <label className="field__label" htmlFor={id}>
        {label}
        {required && (
          <span className="field__required" aria-hidden="true">
            {' '}
            *
          </span>
        )}
      </label>
      {children}
      {error ? (
        <p className="field__error" id={`${id}-error`} role="alert">
          {error}
        </p>
      ) : (
        hint && (
          <p className="field__hint" id={`${id}-hint`}>
            {hint}
          </p>
        )
      )}
    </div>
  )
}

function describedBy(id: string, error: string | undefined, hint: ReactNode) {
  if (error) {
    return `${id}-error`
  }
  return hint ? `${id}-hint` : undefined
}

interface TextFieldProps extends InputHTMLAttributes<HTMLInputElement> {
  label: string
  error?: string | undefined
  hint?: ReactNode
}

export const TextField = forwardRef<HTMLInputElement, TextFieldProps>(function TextField(
  { label, error, hint, required, id: providedId, className, ...rest },
  ref,
) {
  const generatedId = useId()
  const id = providedId ?? generatedId

  return (
    <FieldShell id={id} label={label} error={error} hint={hint} required={required}>
      <input
        ref={ref}
        id={id}
        className={['input', className].filter(Boolean).join(' ')}
        aria-invalid={error ? true : undefined}
        aria-required={required || undefined}
        aria-describedby={describedBy(id, error, hint)}
        {...rest}
      />
    </FieldShell>
  )
})

interface TextAreaFieldProps extends TextareaHTMLAttributes<HTMLTextAreaElement> {
  label: string
  error?: string | undefined
  hint?: ReactNode
}

export const TextAreaField = forwardRef<HTMLTextAreaElement, TextAreaFieldProps>(
  function TextAreaField({ label, error, hint, required, id: providedId, ...rest }, ref) {
    const generatedId = useId()
    const id = providedId ?? generatedId

    return (
      <FieldShell id={id} label={label} error={error} hint={hint} required={required}>
        <textarea
          ref={ref}
          id={id}
          className="input input--textarea"
          aria-invalid={error ? true : undefined}
          aria-required={required || undefined}
          aria-describedby={describedBy(id, error, hint)}
          {...rest}
        />
      </FieldShell>
    )
  },
)

interface SelectFieldProps extends SelectHTMLAttributes<HTMLSelectElement> {
  label: string
  error?: string | undefined
  hint?: ReactNode
}

export const SelectField = forwardRef<HTMLSelectElement, SelectFieldProps>(function SelectField(
  { label, error, hint, required, id: providedId, children, ...rest },
  ref,
) {
  const generatedId = useId()
  const id = providedId ?? generatedId

  return (
    <FieldShell id={id} label={label} error={error} hint={hint} required={required}>
      <select
        ref={ref}
        id={id}
        className="input input--select"
        aria-invalid={error ? true : undefined}
        aria-required={required || undefined}
        aria-describedby={describedBy(id, error, hint)}
        {...rest}
      >
        {children}
      </select>
    </FieldShell>
  )
})

interface CheckboxFieldProps extends Omit<InputHTMLAttributes<HTMLInputElement>, 'type'> {
  label: string
  hint?: ReactNode
}

export const CheckboxField = forwardRef<HTMLInputElement, CheckboxFieldProps>(
  function CheckboxField({ label, hint, id: providedId, ...rest }, ref) {
    const generatedId = useId()
    const id = providedId ?? generatedId

    return (
      <div className="field field--checkbox">
        <input ref={ref} id={id} type="checkbox" className="checkbox" {...rest} />
        <div>
          <label className="field__label" htmlFor={id}>
            {label}
          </label>
          {hint && <p className="field__hint">{hint}</p>}
        </div>
      </div>
    )
  },
)
