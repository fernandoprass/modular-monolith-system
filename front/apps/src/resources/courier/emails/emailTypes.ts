export const EMAIL_QUERY_PARAMS = {
  dateFrom: 'DateFrom',
  dateTo: 'DateTo',
  feature: 'Feature',
  module: 'Module',
  organizationId: 'OrganizationId',
  pageNumber: 'PageNumber',
  pageSize: 'PageSize',
  recipient: 'Recipient',
  subject: 'Subject',
  userId: 'UserId',
} as const

export const EMAIL_REQUEST_FIELDS = {
  body: 'Body',
  feature: 'Feature',
  isHtml: 'IsHtml',
  module: 'Module',
  organizationId: 'OrganizationId',
  recipient: 'Recipient',
  subject: 'Subject',
  templateKey: 'TemplateKey',
  userId: 'UserId',
} as const

export const EMAIL_FILTER_VALUES = {
  all: 'all',
} as const

export const EMAIL_MODULE_OPTIONS = [
  { labelKey: 'modules.iam', value: 'iam' },
  { labelKey: 'modules.courier', value: 'courier' },
  { labelKey: 'modules.sentinel', value: 'sentinel' },
  { labelKey: 'modules.shared', value: 'shared' },
] as const

export const EMAIL_FEATURE_OPTIONS = [
  { labelKey: 'shared.featureLabels.authentication', value: 'authentication' },
  { labelKey: 'shared.featureLabels.emails', value: 'emails' },
  { labelKey: 'shared.featureLabels.organizations', value: 'organizations' },
  { labelKey: 'shared.featureLabels.parameters', value: 'parameters' },
  { labelKey: 'shared.featureLabels.permissions', value: 'permissions' },
  { labelKey: 'shared.featureLabels.roles', value: 'roles' },
  { labelKey: 'shared.featureLabels.security', value: 'security' },
  { labelKey: 'shared.featureLabels.users', value: 'users' },
] as const

export const EMAIL_STATUSES = {
  pending: 1,
  processing: 2,
  sent: 3,
  failed: 4,
} as const

export type EmailStatus = typeof EMAIL_STATUSES[keyof typeof EMAIL_STATUSES]

export type EmailLiteDto = {
  createdAt: string
  feature: string
  id: string
  module: string
  recipient: string
  status: EmailStatus
  subject: string
}

export type DeliveryAttemptDto = {
  attemptedAt: string
  errorMessage: string
  stackTrace: string | null
}

export type EmailDto = EmailLiteDto & {
  attempts: DeliveryAttemptDto[]
  body: string
  expiresAt: string
  isHtml: boolean
  nextAttemptAt: string | null
  organizationId: string
  retryCount: number
  sentAt: string | null
  templateKey: string
  userId: string
}

export type EmailSearchForm = {
  dateFrom: string
  dateTo: string
  feature: string
  module: string
  organizationId: string
  recipient: string
  subject: string
  userId: string
}

export type EmailListQuery = EmailSearchForm & {
  pageNumber: number
  pageSize: number
}

export type EmailCreateForm = {
  body: string
  feature: string
  isHtml: boolean
  module: string
  organizationId: string
  recipient: string
  subject: string
  templateKey: string
  userId: string
}

export type EmailCreateRequest = {
  Body: string
  Feature: string
  IsHtml: boolean
  Module: string
  OrganizationId: string
  Recipient: string
  Subject: string
  TemplateKey: string
  UserId: string
}

export type EmailCreateDto = {
  id: string
}
