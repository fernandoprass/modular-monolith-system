export const AUDIT_LOG_QUERY_PARAMS = {
  action: 'Action',
  feature: 'Feature',
  from: 'From',
  module: 'Module',
  organizationId: 'OrganizationId',
  pageNumber: 'PageNumber',
  pageSize: 'PageSize',
  targetId: 'TargetId',
  to: 'To',
  userId: 'UserId',
} as const

export const AUDIT_LOG_FILTER_VALUES = {
  all: 'all',
} as const

export const AUDIT_LOG_MODULE_OPTIONS = [
  { labelKey: 'modules.iam', value: 'iam' },
  { labelKey: 'modules.shared', value: 'shared' },
  { labelKey: 'modules.courier', value: 'courier' },
  { labelKey: 'modules.sentinel', value: 'sentinel' },
] as const

export const AUDIT_LOG_FEATURE_OPTIONS = [
  { labelKey: 'features.sentinel.auditLogs.features.authentication', value: 'authentication' },
  { labelKey: 'features.sentinel.auditLogs.features.emails', value: 'emails' },
  { labelKey: 'features.sentinel.auditLogs.features.organizations', value: 'organizations' },
  { labelKey: 'features.sentinel.auditLogs.features.parameters', value: 'parameters' },
  { labelKey: 'features.sentinel.auditLogs.features.permissions', value: 'permissions' },
  { labelKey: 'features.sentinel.auditLogs.features.roles', value: 'roles' },
  { labelKey: 'features.sentinel.auditLogs.features.security', value: 'security' },
  { labelKey: 'features.sentinel.auditLogs.features.users', value: 'users' },
] as const

export const AUDIT_PRIVACY_LEVELS = {
  low: 0,
  medium: 1,
  high: 2,
  confidential: 3,
} as const

export type AuditPrivacyLevel = typeof AUDIT_PRIVACY_LEVELS[keyof typeof AUDIT_PRIVACY_LEVELS]

export type AuditLogLiteDto = {
  action: string
  createdAt: string
  description: string
  feature: string
  id: string
  module: string
  privacyLevel: AuditPrivacyLevel
}

export type AuditLogDto = AuditLogLiteDto & {
  expiresAt: string
  ipAddress: string | null
  metadata: string
  organizationId: string
  targetId: string
  userId: string
  userAgent: string | null
}

export type AuditLogSearchForm = {
  action: string
  feature: string
  from: string
  module: string
  targetId: string
  to: string
  userId: string
}

export type AuditLogListQuery = AuditLogSearchForm & {
  organizationId: string
  pageNumber: number
  pageSize: number
}
