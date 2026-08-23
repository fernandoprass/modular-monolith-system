import type { Translate } from '../../../app/i18n/i18n'
import { AUDIT_PRIVACY_LEVELS, type AuditPrivacyLevel } from './auditLogTypes'

const AUDIT_PRIVACY_LEVEL_LABEL_KEYS: Record<AuditPrivacyLevel, string> = {
  [AUDIT_PRIVACY_LEVELS.low]: 'shared.enums.auditPrivacyLevel.low',
  [AUDIT_PRIVACY_LEVELS.medium]: 'shared.enums.auditPrivacyLevel.medium',
  [AUDIT_PRIVACY_LEVELS.high]: 'shared.enums.auditPrivacyLevel.high',
  [AUDIT_PRIVACY_LEVELS.confidential]: 'shared.enums.auditPrivacyLevel.confidential',
}

export function getAuditPrivacyLevelLabel(value: AuditPrivacyLevel, t: Translate): string {
  return t(AUDIT_PRIVACY_LEVEL_LABEL_KEYS[value] ?? 'shared.enums.auditPrivacyLevel.unknown')
}
