import type { Translate } from '../../../app/i18n/i18n'
import { LANGUAGE_OPTIONS } from '../../../shared/languages'

export function toTranslatedOptions(
  options: ReadonlyArray<{ labelKey: string, value: string }>,
  t: Translate,
) {
  return options.map((option) => ({
    label: t(option.labelKey),
    value: option.value,
  }))
}

export function getLanguageLabel(language: string, t: Translate): string {
  const option = LANGUAGE_OPTIONS.find((item) => item.value === language)

  return option === undefined ? language : t(option.labelKey)
}
