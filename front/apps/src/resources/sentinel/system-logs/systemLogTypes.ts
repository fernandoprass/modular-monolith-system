export const SYSTEM_LOG_QUERY_PARAMS = {
  from: 'From',
  level: 'Level',
  organizationId: 'OrganizationId',
  pageNumber: 'PageNumber',
  pageSize: 'PageSize',
  requestId: 'RequestId',
  status: 'Status',
  to: 'To',
  userId: 'UserId',
} as const

export const SYSTEM_LOG_FILTER_VALUES = {
  all: 'all',
} as const

export const SYSTEM_LOG_LEVELS = {
  debug: 0,
  information: 1,
  warning: 2,
  error: 3,
  critical: 4,
} as const

export const SYSTEM_LOG_STATUSES = {
  unknown: 0,
  success: 1,
  failure: 2,
  unauthorized: 3,
} as const

export const SYSTEM_LOG_LEVEL_OPTIONS = [
  { labelKey: 'features.sentinel.systemLogs.levels.debug', value: SYSTEM_LOG_LEVELS.debug.toString() },
  { labelKey: 'features.sentinel.systemLogs.levels.information', value: SYSTEM_LOG_LEVELS.information.toString() },
  { labelKey: 'features.sentinel.systemLogs.levels.warning', value: SYSTEM_LOG_LEVELS.warning.toString() },
  { labelKey: 'features.sentinel.systemLogs.levels.error', value: SYSTEM_LOG_LEVELS.error.toString() },
  { labelKey: 'features.sentinel.systemLogs.levels.critical', value: SYSTEM_LOG_LEVELS.critical.toString() },
] as const

export const SYSTEM_LOG_STATUS_OPTIONS = [
  { labelKey: 'features.sentinel.systemLogs.statuses.unknown', value: SYSTEM_LOG_STATUSES.unknown.toString() },
  { labelKey: 'features.sentinel.systemLogs.statuses.success', value: SYSTEM_LOG_STATUSES.success.toString() },
  { labelKey: 'features.sentinel.systemLogs.statuses.failure', value: SYSTEM_LOG_STATUSES.failure.toString() },
  { labelKey: 'features.sentinel.systemLogs.statuses.unauthorized', value: SYSTEM_LOG_STATUSES.unauthorized.toString() },
] as const

export type SystemLogLevel = typeof SYSTEM_LOG_LEVELS[keyof typeof SYSTEM_LOG_LEVELS]

export type SystemLogStatus = typeof SYSTEM_LOG_STATUSES[keyof typeof SYSTEM_LOG_STATUSES]

export type SystemLogLiteDto = {
  createdAt: string
  id: string
  level: SystemLogLevel
  message: string
  module: string
  status: SystemLogStatus
}

export type SystemLogDto = SystemLogLiteDto & {
  exception: string | null
  expiresAt: string
  organizationId: string | null
  propertiesJson: string
  requestId: string | null
  stackTrace: string | null
  userId: string | null
}

export type SystemLogSearchForm = {
  from: string
  level: string
  organizationId: string
  requestId: string
  status: string
  to: string
  userId: string
}

export type SystemLogListQuery = SystemLogSearchForm & {
  pageNumber: number
  pageSize: number
}
