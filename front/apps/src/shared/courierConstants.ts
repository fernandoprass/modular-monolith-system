export const COURIER_MODULE = 'courier'

export const COURIER_RESOURCES = {
  emails: 'emails',
  notifications: 'notifications',
  templates: 'templates',
} as const

export const COURIER_ACTIONS = {
  read: 'read',
  write: 'write',
} as const

function permission(resource: string, action: string): string {
  return `${COURIER_MODULE}.${resource}.${action}`
}

export const COURIER_PERMISSIONS = {
  emails: {
    read: permission(COURIER_RESOURCES.emails, COURIER_ACTIONS.read),
    write: permission(COURIER_RESOURCES.emails, COURIER_ACTIONS.write),
  },
  notifications: {
    read: permission(COURIER_RESOURCES.notifications, COURIER_ACTIONS.read),
    write: permission(COURIER_RESOURCES.notifications, COURIER_ACTIONS.write),
  },
  templates: {
    read: permission(COURIER_RESOURCES.templates, COURIER_ACTIONS.read),
    write: permission(COURIER_RESOURCES.templates, COURIER_ACTIONS.write),
  },
} as const
