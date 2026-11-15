import type { Translate } from '../../../app/i18n/i18n'
import { ORGANIZATION_TYPE_OPTIONS } from './organizationTypes'

export function getOrganizationTypeEnum(type: number, translate: Translate): string {
  const option = ORGANIZATION_TYPE_OPTIONS.find((item) => item.value === String(type))

  return option === undefined ? String(type) : translate(option.labelKey)
}
