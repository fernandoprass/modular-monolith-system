import type { Translate } from '../../../app/i18n/i18n'
import {
  RETENTION_POLICIES,
  TEMPLATE_TYPES,
  type RetentionPolicy,
  type TemplateType,
} from './templateTypes'

type TemplateOption = {
  labelKey: string
  value: string
}

const TEMPLATE_TYPE_LABEL_KEYS: Record<TemplateType, string> = {
  [TEMPLATE_TYPES.comment]: 'features.courier.templates.types.comment',
  [TEMPLATE_TYPES.email]: 'features.courier.templates.types.email',
  [TEMPLATE_TYPES.notification]: 'features.courier.templates.types.notification',
}

const RETENTION_POLICY_LABEL_KEYS: Record<RetentionPolicy, string> = {
  [RETENTION_POLICIES.operational]: 'features.courier.templates.retentionPolicies.operational',
  [RETENTION_POLICIES.standard]: 'features.courier.templates.retentionPolicies.standard',
  [RETENTION_POLICIES.extended]: 'features.courier.templates.retentionPolicies.extended',
  [RETENTION_POLICIES.compliance]: 'features.courier.templates.retentionPolicies.compliance',
  [RETENTION_POLICIES.longTerm]: 'features.courier.templates.retentionPolicies.longTerm',
}

export function getTemplateTypeLabel(value: TemplateType, t: Translate): string {
  return t(TEMPLATE_TYPE_LABEL_KEYS[value])
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
