import type { PlainTranslationKey, Translate } from '../../../app/i18n/i18n'
import { AUDIT_PRIVACY_LEVELS, type AuditPrivacyLevel } from './auditLogTypes'

const AUDIT_PRIVACY_LEVEL_ENUM_KEYS: Record<AuditPrivacyLevel, PlainTranslationKey> = {
  [AUDIT_PRIVACY_LEVELS.low]: 'common.enums.auditPrivacyLevel.low',
  [AUDIT_PRIVACY_LEVELS.medium]: 'common.enums.auditPrivacyLevel.medium',
  [AUDIT_PRIVACY_LEVELS.high]: 'common.enums.auditPrivacyLevel.high',
  [AUDIT_PRIVACY_LEVELS.confidential]: 'common.enums.auditPrivacyLevel.confidential',
}

export function getAuditPrivacyLevelEnum(value: AuditPrivacyLevel, t: Translate): string {
  return t(AUDIT_PRIVACY_LEVEL_ENUM_KEYS[value] ?? 'common.enums.auditPrivacyLevel.unknown')
}
