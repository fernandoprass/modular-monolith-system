export const TEMPLATE_QUERY_PARAMS = {
  key: 'Key',
  module: 'Module',
  name: 'Name',
  pageNumber: 'PageNumber',
  pageSize: 'PageSize',
  severity: 'Severity',
} as const

export const TEMPLATE_REQUEST_FIELDS = {
  isAllowingOptOut: 'IsAllowingOptOut',
  key: 'Key',
  module: 'Module',
  retentionPolicy: 'RetentionPolicy',
  severity: 'Severity',
} as const

export const TEMPLATE_FILTER_VALUES = {
  all: 'all',
} as const

export const TEMPLATE_MODULE_OPTIONS = [
  { labelKey: 'modules.iam', value: 'iam' },
  { labelKey: 'modules.shared', value: 'shared' },
  { labelKey: 'modules.sentinel', value: 'sentinel' },
  { labelKey: 'modules.courier', value: 'courier' },
] as const

export const NOTIFICATION_SEVERITIES = {
  critical: 3,
  information: 1,
  warning: 2,
} as const

export const NOTIFICATION_SEVERITY_OPTIONS = [
  { labelKey: 'features.courier.templates.severities.information', value: String(NOTIFICATION_SEVERITIES.information) },
  { labelKey: 'features.courier.templates.severities.warning', value: String(NOTIFICATION_SEVERITIES.warning) },
  { labelKey: 'features.courier.templates.severities.critical', value: String(NOTIFICATION_SEVERITIES.critical) },
] as const

export const RETENTION_POLICIES = {
  operational: 1,
  standard: 2,
  extended: 3,
  compliance: 4,
  longTerm: 5,
} as const

export const RETENTION_POLICY_OPTIONS = [
  { labelKey: 'features.courier.templates.retentionPolicies.operational', value: String(RETENTION_POLICIES.operational) },
  { labelKey: 'features.courier.templates.retentionPolicies.standard', value: String(RETENTION_POLICIES.standard) },
  { labelKey: 'features.courier.templates.retentionPolicies.extended', value: String(RETENTION_POLICIES.extended) },
  { labelKey: 'features.courier.templates.retentionPolicies.compliance', value: String(RETENTION_POLICIES.compliance) },
  { labelKey: 'features.courier.templates.retentionPolicies.longTerm', value: String(RETENTION_POLICIES.longTerm) },
] as const

export type NotificationSeverity = typeof NOTIFICATION_SEVERITIES[keyof typeof NOTIFICATION_SEVERITIES]
export type RetentionPolicy = typeof RETENTION_POLICIES[keyof typeof RETENTION_POLICIES]

export type TemplateLiteDto = {
  id: string
  isAllowingOptOut: boolean
  key: string
  module: string
  name: string
  retentionPolicy: RetentionPolicy
  severity: NotificationSeverity
}

export type TemplateTranslationEmailDto = {
  body: string
  isHtml: boolean
  subject: string
}

export type TemplateTranslationNotificationDto = {
  actionLink: string | null
  message: string
  title: string
}

export type TemplateTranslationDto = {
  email: TemplateTranslationEmailDto | null
  language: string
  name: string
  notification: TemplateTranslationNotificationDto | null
}

export type TemplateDto = Omit<TemplateLiteDto, 'name'> & {
  createdAt: string
  createdBy: string
  translations: TemplateTranslationDto[]
  updatedAt: string | null
  updatedBy: string | null
}

export type TemplateSearchForm = {
  key: string
  module: string
  name: string
  severity: string
}

export type TemplateListQuery = TemplateSearchForm & {
  pageNumber: number
  pageSize: number
}

export type TemplateForm = {
  isAllowingOptOut: boolean
  key: string
  module: string
  retentionPolicy: string
  severity: string
}

export type TemplateRequest = {
  IsAllowingOptOut: boolean
  Key: string
  Module: string
  RetentionPolicy: number
  Severity: number
}

export type TemplateTranslationForm = {
  emailBody: string
  emailEnabled: boolean
  emailSubject: string
  language: string
  name: string
  notificationActionLink: string
  notificationEnabled: boolean
  notificationMessage: string
  notificationTitle: string
}

export type TemplateTranslationRequest = {
  Email: {
    Body: string
    Subject: string
  } | null
  Language: string
  Name: string
  Notification: {
    ActionLink: string | null
    Message: string
    Title: string
  } | null
}
