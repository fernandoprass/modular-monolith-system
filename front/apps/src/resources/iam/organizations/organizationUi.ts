import type { PlainTranslationKey, Translate } from '../../../app/i18n/i18n'
import { LANGUAGE_OPTIONS } from '../../../shared/languages'
import { ORGANIZATION_TYPE_OPTIONS } from './organizationTypes'

export function toTranslatedOptions(
  options: readonly { labelKey: PlainTranslationKey; value: string }[],
  translate: Translate,
): { label: string; value: string }[] {
  return options.map((option) => ({
    label: translate(option.labelKey),
    value: option.value,
  }))
}

export function getOrganizationTypeEnum(type: number, translate: Translate): string {
  const option = ORGANIZATION_TYPE_OPTIONS.find((item) => item.value === String(type))

  return option === undefined ? String(type) : translate(option.labelKey)
}

export function getLanguageLabel(code: string, translate: Translate): string {
  const option = LANGUAGE_OPTIONS.find((item) => item.value === code)

  return option === undefined ? code : translate(option.labelKey)
}
