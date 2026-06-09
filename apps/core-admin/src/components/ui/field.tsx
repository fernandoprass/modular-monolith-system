import type { ReactNode } from 'react'

import { Label } from './label'

type FieldProps = {
  children: ReactNode
  label: string
}

export function Field({ children, label }: FieldProps) {
  return (
    <div className="field">
      <Label>{label}</Label>
      {children}
    </div>
  )
}
