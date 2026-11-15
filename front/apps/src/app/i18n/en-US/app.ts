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

export const RESOURCE_LABELS = {
  courier: {
    emailTemplates: 'Email templates',
    emails: 'Emails',
    templates: 'Templates',
  },
  sentinel: {
    auditLogs: 'Audit logs',
    systemLogs: 'System logs',
  },
  iam: {
    organization: 'Organization',
    organizationProfile: 'Organization profile',
    organizations: 'Organizations',
    parameter: 'Parameter',
    parameters: 'Parameters',
    permission: 'Permission',
    permissions: 'Permissions',
    role: 'Role',
    roles: 'Roles',
    userProfile: 'User profile',
    users: 'Users',
  },
} as const

export const appMessages = {
  app: {
    dashboard: {
      subtitle: 'Admin workspace for Core API modules.',
      title: 'Core Admin',
    },
  },
  auth: {
    login: {
      email: 'Email',
      password: 'Password',
      signUp: 'New here? Sign up',
      submit: 'Sign in',
      title: 'Sign in',
    },
    userMenu: {
      logout: 'Logout',
    },
  },
  navigation: {
    dashboard: 'Dashboard',
    menu: 'Menu',
    profile: 'Organization Profile',
    settings: 'Settings',
    toggleSidebar: 'Toggle sidebar',
    groups: {
      authorization: 'Authorization',
      courier: MODULE_LABELS.courier,
      iam: MODULE_LABELS.iam,
      sentinel: MODULE_LABELS.sentinel,
    },
  },
  public: {
    organizationRegistration: {
      actions: {
        signIn: 'Back to sign in',
        submit: 'Create organization',
      },
      fields: {
        adminEmail: 'Admin email',
        adminName: 'Admin name',
        adminPassword: 'Admin password',
      },
      title: 'Create organization',
    },
  },
} as const
