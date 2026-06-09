import type { FormEvent, ReactNode } from 'react'

import { useTranslate } from '../../app/i18n/i18n'
import { Button } from './button'

type FilterToolbarProps = {
  children: ReactNode
  onReset: () => void
  onSubmit: (event: FormEvent<HTMLFormElement>) => void
}

export function FilterToolbar({ children, onReset, onSubmit }: FilterToolbarProps) {
  const t = useTranslate()

  return (
    <form className="toolbar" onSubmit={onSubmit}>
      {children}
      <Button type="submit">{t('shared.actions.filter')}</Button>
      <Button onClick={onReset} type="button" variant="outline">{t('shared.actions.reset')}</Button>
    </form>
  )
}
