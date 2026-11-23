import type { PlainTranslationKey, Translate } from '../../../app/i18n/i18n'
import { LOOKUP_TYPE_ENUM, type LookupType } from './lookupTypes'

export const LOOKUP_TYPE_ENUM_KEYS: Record<LookupType, PlainTranslationKey> = {
  [LOOKUP_TYPE_ENUM.country]: 'features.shared.enums.lookupType.country',
  [LOOKUP_TYPE_ENUM.state]: 'features.shared.enums.lookupType.state',
}

export function getLookupTypeEnum(value: string, t: Translate): string {
  switch (value) {
    case LOOKUP_TYPE_ENUM.country: return t(LOOKUP_TYPE_ENUM_KEYS.country)
    case LOOKUP_TYPE_ENUM.state: return t(LOOKUP_TYPE_ENUM_KEYS.state)
    default: return value
  }
}
