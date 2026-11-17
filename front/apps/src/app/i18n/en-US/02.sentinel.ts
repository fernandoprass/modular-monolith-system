import { SYSTEM_LOG_LEVEL_ENUM, SYSTEM_LOG_STATUS_ENUM } from './enums'

export const sentinelMessages = {
  enums: {
    systemLogLevel: SYSTEM_LOG_LEVEL_ENUM,
    systemLogStatus: SYSTEM_LOG_STATUS_ENUM,
  },
  labels: {
    auditLog: 'Audit log',
    auditLogs: 'Audit logs',
    exception: 'Exception',
    ipAddress: 'IP address',
    level: 'Level',
    metadata: 'Metadata',
    privacyLevel: 'Privacy level',
    propertiesJson: 'Properties JSON',
    requestId: 'Request ID',
    systemLog: 'System log',
    systemLogs: 'System logs',
    targetId: 'Target ID',
    userAgent: 'User agent',
  },
} as const
