import type { PlainTranslationKey, Translate } from '../../../app/i18n/i18n'
import {
  NOTIFICATION_SEVERITIES,
  RETENTION_POLICIES,
  type NotificationSeverity,
  type RetentionPolicy,
} from './templateTypes'

const NOTIFICATION_SEVERITY_ENUM_KEYS: Record<NotificationSeverity, PlainTranslationKey> = {
  [NOTIFICATION_SEVERITIES.information]: 'features.courier.enums.notificationSeverity.information',
  [NOTIFICATION_SEVERITIES.warning]: 'features.courier.enums.notificationSeverity.warning',
  [NOTIFICATION_SEVERITIES.critical]: 'features.courier.enums.notificationSeverity.critical',
}

const RETENTION_POLICY_ENUM_KEYS: Record<RetentionPolicy, PlainTranslationKey> = {
  [RETENTION_POLICIES.operational]: 'common.enums.retentionPolicy.operational',
  [RETENTION_POLICIES.standard]: 'common.enums.retentionPolicy.standard',
  [RETENTION_POLICIES.extended]: 'common.enums.retentionPolicy.extended',
  [RETENTION_POLICIES.compliance]: 'common.enums.retentionPolicy.compliance',
  [RETENTION_POLICIES.longTerm]: 'common.enums.retentionPolicy.longTerm',
}

export function getNotificationSeverityEnum(value: NotificationSeverity, t: Translate): string {
  return t(NOTIFICATION_SEVERITY_ENUM_KEYS[value])
}

export function getRetentionPolicyEnum(value: RetentionPolicy, t: Translate): string {
  return t(RETENTION_POLICY_ENUM_KEYS[value])
}
