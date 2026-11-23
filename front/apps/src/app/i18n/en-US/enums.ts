// Grouped by backend definition ownership, regardless of where the enum is used.

// 01. Shared
export const LOOKUP_TYPE_ENUM = {
  country: 'Country',
  state: 'State',
} as const

export const AUDIT_PRIVACY_LEVEL_ENUM = {
  confidential: 'Confidential',
  high: 'High',
  low: 'Low',
  medium: 'Medium',
  unknown: 'Unknown',
} as const

export const PARAMETER_OVERRIDE_TYPE_ENUM = {
  none: 'None',
  organizationId: 'Organization',
  userId: 'User',
} as const

export const PARAMETER_TYPE_ENUM = {
  boolean: 'Boolean',
  character: 'Character',
  date: 'Date',
  dateTime: 'Date time',
  decimal: 'Decimal',
  integer: 'Integer',
  list: 'List',
  referenceId: 'Reference ID',
  richText: 'Rich text',
  string: 'String',
  text: 'Text',
  time: 'Time',
  uuid: 'UUID',
} as const

export const RETENTION_POLICY_ENUM = {
  compliance: 'Compliance',
  extended: 'Extended',
  longTerm: 'Long term',
  operational: 'Operational',
  standard: 'Standard',
} as const

export const SYSTEM_LOG_LEVEL_ENUM = {
  critical: 'Critical',
  debug: 'Debug',
  error: 'Error',
  information: 'Information',
  unknown: 'Unknown',
  warning: 'Warning',
} as const

export const SYSTEM_LOG_STATUS_ENUM = {
  failure: 'Failure',
  success: 'Success',
  unauthorized: 'Unauthorized',
  unknown: 'Unknown',
} as const

// 02. Sentinel: uses Shared enums; no module-owned enums.

// 03. IAM
export const ORGANIZATION_TYPE_ENUM = {
  company: 'Company',
  individual: 'Individual',
} as const

// 04. Courier
export const EMAIL_STATUS_ENUM = {
  failed: 'Failed',
  pending: 'Pending',
  processing: 'Processing',
  sent: 'Sent',
} as const

export const NOTIFICATION_SEVERITY_ENUM = {
  critical: 'Critical',
  information: 'Information',
  warning: 'Warning',
} as const

export const NOTIFICATION_STATUS_ENUM = {
  read: 'Read',
  unread: 'Unread',
} as const
