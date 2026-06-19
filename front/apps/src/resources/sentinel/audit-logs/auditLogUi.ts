import type { Translate } from '../../../app/i18n/i18n'
import { AUDIT_PRIVACY_LEVELS, type AuditPrivacyLevel } from './auditLogTypes'

const AUDIT_PRIVACY_LEVEL_LABEL_KEYS: Record<AuditPrivacyLevel, string> = {
  [AUDIT_PRIVACY_LEVELS.low]: 'features.sentinel.auditLogs.privacyLevels.low',
  [AUDIT_PRIVACY_LEVELS.medium]: 'features.sentinel.auditLogs.privacyLevels.medium',
  [AUDIT_PRIVACY_LEVELS.high]: 'features.sentinel.auditLogs.privacyLevels.high',
  [AUDIT_PRIVACY_LEVELS.confidential]: 'features.sentinel.auditLogs.privacyLevels.confidential',
}

export function getAuditPrivacyLevelLabel(value: AuditPrivacyLevel, t: Translate): string {
  return t(AUDIT_PRIVACY_LEVEL_LABEL_KEYS[value] ?? 'features.sentinel.auditLogs.privacyLevels.unknown')
}
