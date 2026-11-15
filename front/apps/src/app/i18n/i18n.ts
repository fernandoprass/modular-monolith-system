import { createContext, useContext } from 'react'
import { DEFAULT_LOCALE, translate, type SupportedLocale, type Translate } from './messages'

export * from './messages'

export const LocaleContext = createContext<SupportedLocale>(DEFAULT_LOCALE)
export const I18nContext = createContext<Translate>(translate)

export function useLocale(): SupportedLocale {
  return useContext(LocaleContext)
}

export function useTranslate(): Translate {
  return useContext(I18nContext)
}
