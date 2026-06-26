export const SENTINEL_MODULE = 'sentinel'

export const SENTINEL_RESOURCES = {
  auditlogs: 'auditlogs',
  systemlogs: 'systemlogs',
} as const

export const SENTINEL_ACTIONS = {
  read: 'read',
} as const

function permission(resource: string, action: string): string {
  return `${SENTINEL_MODULE}.${resource}.${action}`
}

export const SENTINEL_PERMISSIONS = {
  auditLogs: {
    read: permission(SENTINEL_RESOURCES.auditlogs, SENTINEL_ACTIONS.read),
  },
  systemLogs: {
    read: permission(SENTINEL_RESOURCES.systemlogs, SENTINEL_ACTIONS.read),
  },
} as const
