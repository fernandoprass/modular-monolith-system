import type { PlainTranslationKey, Translate } from '../app/i18n/messages'
import { ALL_LANGUAGE_OPTIONS, normalizeLanguageCode } from './languages'

export type TranslationOption = {
  labelKey: PlainTranslationKey
  value: string
}

export function toTranslatedOptions<T extends TranslationOption>(options: readonly T[], t: Translate) {
  return options.map(option => ({ label: t(option.labelKey), value: option.value }))
}

export function getLanguageLabel(language: string, t: Translate): string {
  const normalized = normalizeLanguageCode(language)
  const option = ALL_LANGUAGE_OPTIONS.find(item => item.value === normalized)
  return option === undefined ? language : t(option.labelKey)
}

