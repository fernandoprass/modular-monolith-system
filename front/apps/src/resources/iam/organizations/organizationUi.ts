import { LANGUAGE_OPTIONS } from '../../../shared/languages'
import { ORGANIZATION_TYPE_OPTIONS } from './organizationTypes'

export function toTranslatedOptions(
  options: readonly { labelKey: string; value: string }[],
  translate: (key: string) => string,
): { label: string; value: string }[] {
  return options.map((option) => ({
    label: translate(option.labelKey),
    value: option.value,
  }))
}

export function getOrganizationTypeLabel(type: number, translate: (key: string) => string): string {
  const option = ORGANIZATION_TYPE_OPTIONS.find((item) => item.value === String(type))

  return option === undefined ? String(type) : translate(option.labelKey)
}

export function getLanguageLabel(code: string, translate: (key: string) => string): string {
  const option = LANGUAGE_OPTIONS.find((item) => item.value === code)

  return option === undefined ? code : translate(option.labelKey)
}
