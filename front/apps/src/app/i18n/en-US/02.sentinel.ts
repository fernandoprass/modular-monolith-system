import { RESOURCE_LABELS } from './app'
import { SYSTEM_LOG_LEVEL_ENUM, SYSTEM_LOG_STATUS_ENUM } from './enums'

export const sentinelMessages = {
  auditLogs: {
    messages: {
      empty: 'No audit logs found.',
    },
    name: RESOURCE_LABELS.sentinel.auditLogs,
    pages: {
      list: RESOURCE_LABELS.sentinel.auditLogs,
      show: 'Audit log details',
    },
  },
  systemLogs: {
    levels: SYSTEM_LOG_LEVEL_ENUM,
    messages: {
      empty: 'No system logs found.',
    },
    name: RESOURCE_LABELS.sentinel.systemLogs,
    pages: {
      list: RESOURCE_LABELS.sentinel.systemLogs,
      show: 'System log details',
    },
    statuses: SYSTEM_LOG_STATUS_ENUM,
  },
} as const
