import type { HTMLAttributes, LabelHTMLAttributes, ReactNode } from 'react'

import { Label } from './label'
import { cn } from '../../lib/utils'

type FieldProps = {
  children: ReactNode
} & HTMLAttributes<HTMLDivElement>

export function FieldGroup({ className, ...props }: HTMLAttributes<HTMLDivElement>) {
  return <div className={cn('field-group', className)} {...props} />
}

export function Field({ className, ...props }: FieldProps) {
  return <div className={cn('field', className)} {...props} />
}

export function FieldLabel({ className, ...props }: LabelHTMLAttributes<HTMLLabelElement>) {
  return <Label className={className} {...props} />
}

export function FieldDescription({ className, ...props }: HTMLAttributes<HTMLParagraphElement>) {
  return <p className={cn('field-description', className)} {...props} />
}

export function FieldError({ children, className, ...props }: HTMLAttributes<HTMLParagraphElement>) {
  if (children === undefined || children === null || children === '') {
    return null
  }

  return (
    <p className={cn('field-error', className)} {...props}>
      {children}
    </p>
  )
}
