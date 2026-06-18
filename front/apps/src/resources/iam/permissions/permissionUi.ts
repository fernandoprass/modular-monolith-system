import type { Translate } from '../../../app/i18n/i18n'

export type TranslationOption = {
  labelKey: string
  value: string
}

export function toTranslatedOptions<TOption extends TranslationOption>(
  options: readonly TOption[],
  t: Translate,
) {
  return options.map((option) => ({
    label: t(option.labelKey),
    value: option.value,
  }))
}
