import type { Translate } from '../../../app/i18n/i18n'
import {
  NOTIFICATION_SEVERITIES,
  RETENTION_POLICIES,
  type NotificationSeverity,
  type RetentionPolicy,
} from './templateTypes'

type TemplateOption = {
  labelKey: string
  value: string
}

const NOTIFICATION_SEVERITY_LABEL_KEYS: Record<NotificationSeverity, string> = {
  [NOTIFICATION_SEVERITIES.information]: 'shared.enums.notificationSeverity.information',
  [NOTIFICATION_SEVERITIES.warning]: 'shared.enums.notificationSeverity.warning',
  [NOTIFICATION_SEVERITIES.critical]: 'shared.enums.notificationSeverity.critical',
}

const RETENTION_POLICY_LABEL_KEYS: Record<RetentionPolicy, string> = {
  [RETENTION_POLICIES.operational]: 'shared.enums.retentionPolicy.operational',
  [RETENTION_POLICIES.standard]: 'shared.enums.retentionPolicy.standard',
  [RETENTION_POLICIES.extended]: 'shared.enums.retentionPolicy.extended',
  [RETENTION_POLICIES.compliance]: 'shared.enums.retentionPolicy.compliance',
  [RETENTION_POLICIES.longTerm]: 'shared.enums.retentionPolicy.longTerm',
}

export function getNotificationSeverityLabel(value: NotificationSeverity, t: Translate): string {
  return t(NOTIFICATION_SEVERITY_LABEL_KEYS[value])
}

export function getRetentionPolicyLabel(value: RetentionPolicy, t: Translate): string {
  return t(RETENTION_POLICY_LABEL_KEYS[value])
}

export function toTranslatedTemplateOptions(options: readonly TemplateOption[], t: Translate) {
  return options.map((option) => ({
    label: t(option.labelKey),
    value: option.value,
  }))
}
