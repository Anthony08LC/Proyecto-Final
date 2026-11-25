import type { ButtonHTMLAttributes } from 'react'
import { Link, type LinkProps } from 'react-router'

type Variant = 'primary' | 'secondary' | 'danger' | 'ghost'

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: Variant
  isLoading?: boolean
}

export function Button({
  variant = 'secondary',
  isLoading = false,
  disabled,
  className,
  children,
  type = 'button',
  ...rest
}: ButtonProps) {
  return (
    <button
      type={type}
      className={['button', `button--${variant}`, className].filter(Boolean).join(' ')}
      disabled={disabled || isLoading}
      aria-busy={isLoading || undefined}
      {...rest}
    >
      {isLoading && <span className="spinner spinner--inline" aria-hidden="true" />}
      {children}
    </button>
  )
}

interface ButtonLinkProps extends LinkProps {
  variant?: Variant
}

/** Enlace con aspecto de botón, para navegar (por ejemplo, «Nueva categoría»). */
export function ButtonLink({ variant = 'secondary', className, ...rest }: ButtonLinkProps) {
  return (
    <Link
      className={['button', `button--${variant}`, className].filter(Boolean).join(' ')}
      {...rest}
    />
  )
}
