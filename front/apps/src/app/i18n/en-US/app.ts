export const MODULE_LABELS = {
  courier: 'Courier',
  iam: 'IAM',
  sentinel: 'Sentinel',
  shared: 'Shared',
} as const

export const CORE_FEATURE_LABELS = {
  authentication: 'Authentication',
  emails: 'Emails',
  organizations: 'Organizations',
  parameters: 'Parameters',
  permissions: 'Permissions',
  roles: 'Roles',
  security: 'Security',
  users: 'Users',
} as const

export const appMessages = {
  app: {
    dashboard: {
      subtitle: 'Admin workspace for Core API modules.',
      title: 'Core Admin',
    },
    entities: {
      auditLogs: 'audit logs',
      emails: 'emails',
      languageTranslation: 'language translation',
      lookups: 'lookups',
      notification: 'notification',
      notifications: 'notifications',
      optOutTemplates: 'opt-out templates',
      organization: 'organization',
      organizations: 'organizations',
      parameters: 'parameters',
      permissions: 'permissions',
      role: 'role',
      roles: 'roles',
      systemLogs: 'system logs',
      templates: 'templates',
      translations: 'translations',
      user: 'user',
      users: 'users',
    },
  },
  navigation: {
    dashboard: 'Dashboard',
    groups: {
      authorization: 'Authorization',
      courier: MODULE_LABELS.courier,
      iam: MODULE_LABELS.iam,
      sentinel: MODULE_LABELS.sentinel,
      settings: 'Settings',
    },
    menu: 'Menu',
    profile: 'Organization Profile',
    toggleSidebar: 'Toggle sidebar',
  },
} as const
