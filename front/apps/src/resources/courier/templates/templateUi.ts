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
  [NOTIFICATION_SEVERITIES.information]: 'features.courier.templates.severities.information',
  [NOTIFICATION_SEVERITIES.warning]: 'features.courier.templates.severities.warning',
  [NOTIFICATION_SEVERITIES.critical]: 'features.courier.templates.severities.critical',
}

const RETENTION_POLICY_LABEL_KEYS: Record<RetentionPolicy, string> = {
  [RETENTION_POLICIES.operational]: 'features.courier.templates.retentionPolicies.operational',
  [RETENTION_POLICIES.standard]: 'features.courier.templates.retentionPolicies.standard',
  [RETENTION_POLICIES.extended]: 'features.courier.templates.retentionPolicies.extended',
  [RETENTION_POLICIES.compliance]: 'features.courier.templates.retentionPolicies.compliance',
  [RETENTION_POLICIES.longTerm]: 'features.courier.templates.retentionPolicies.longTerm',
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
