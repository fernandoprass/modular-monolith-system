const MODULE_LABELS = {
  courier: 'Courier',
  iam: 'IAM',
  sentinel: 'Sentinel',
  shared: 'Shared',
} as const

const CORE_FEATURE_LABELS = {
  authentication: 'Authentication',
  emails: 'Emails',
  organizations: 'Organizations',
  parameters: 'Parameters',
  permissions: 'Permissions',
  roles: 'Roles',
  security: 'Security',
  users: 'Users',
} as const

const NOTIFICATION_SEVERITY_LABELS = {
  critical: 'Critical',
  information: 'Information',
  warning: 'Warning',
} as const

const RETENTION_POLICY_LABELS = {
  compliance: 'Compliance',
  extended: 'Extended',
  longTerm: 'Long term',
  operational: 'Operational',
  standard: 'Standard',
} as const

const EMAIL_STATUS_LABELS = {
  failed: 'Failed',
  pending: 'Pending',
  processing: 'Processing',
  sent: 'Sent',
} as const

const NOTIFICATION_STATUS_LABELS = {
  read: 'Read',
  unread: 'Unread',
} as const

const AUDIT_PRIVACY_LEVEL_LABELS = {
  confidential: 'Confidential',
  high: 'High',
  low: 'Low',
  medium: 'Medium',
  unknown: 'Unknown',
} as const

const SYSTEM_LOG_LEVEL_LABELS = {
  critical: NOTIFICATION_SEVERITY_LABELS.critical,
  debug: 'Debug',
  error: 'Error',
  information: NOTIFICATION_SEVERITY_LABELS.information,
  unknown: 'Unknown',
  warning: NOTIFICATION_SEVERITY_LABELS.warning,
} as const

const SYSTEM_LOG_STATUS_LABELS = {
  failure: 'Failure',
  success: 'Success',
  unauthorized: 'Unauthorized',
  unknown: 'Unknown',
} as const

const ORGANIZATION_TYPE_LABELS = {
  company: 'Company',
  individual: 'Individual',
} as const

const PARAMETER_OVERRIDE_TYPE_LABELS = {
  none: 'None',
  organizationId: 'Organization',
  userId: 'User',
} as const

const PARAMETER_TYPE_LABELS = {
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

const RESOURCE_LABELS = {
  courier: {
    emailTemplates: 'Email templates',
    emails: 'Emails',
    notifications: 'Notifications',
    templates: 'Templates',
    userPreferences: 'Preferences',
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

export const enMessages = {
  app: {
    dashboard: {
      subtitle: 'Admin workspace for Core API modules.',
      title: 'Core Admin',
    },
    shell: {
      workspace: 'Admin workspace',
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
        code: 'Code',
        defaultLanguage: 'Default language',
        description: 'Description',
        name: 'Name',
        type: 'Type',
      },
      messages: {
        success: 'Organization created. You can sign in now.',
      },
      title: 'Create organization',
    },
  },
  features: {
    courier: {
      emails: {
        actions: {
          create: 'Create email',
        },
        messages: {
          empty: 'No emails found.',
          noAttempts: 'No delivery attempts recorded.',
        },
        name: RESOURCE_LABELS.courier.emails,
        notifications: {
          created: 'Email created.',
        },
        pages: {
          create: 'Create email',
          list: RESOURCE_LABELS.courier.emails,
          show: 'Email details',
        },
        sections: {
          attempts: 'Delivery attempts',
        },
        statuses: EMAIL_STATUS_LABELS,
      },
      notifications: {
        actions: {
          markRead: 'Mark as read',
          openLink: 'Open link',
        },
        messages: {
          deleteConfirm: 'Delete this notification?',
          deleteTitle: 'Delete notification',
          empty: 'No notifications found.',
        },
        name: RESOURCE_LABELS.courier.notifications,
        notifications: {
          deleted: 'Notification deleted.',
          markedRead: 'Notification marked as read.',
        },
        pages: {
          list: RESOURCE_LABELS.courier.notifications,
        },
        statuses: NOTIFICATION_STATUS_LABELS,
      },
      userPreferences: {
        fields: {
          enableAllEmail: 'Receive email for all templates',
          enableAllNotification: 'Receive notifications for all templates',
          enableEmail: 'Receive email',
          enableNotification: 'Receive notification',
          communications: 'Communications',
        },
        messages: {
          empty: 'No opt-out templates found.',
          communicationsHelp: 'Disable the communications you do not want to receive.',
        },
        name: RESOURCE_LABELS.courier.userPreferences,
        notifications: {
          updated: 'Preferences updated.',
        },
        pages: {
          edit: RESOURCE_LABELS.courier.userPreferences,
        },
        sections: {
          templates: 'Templates',
        },
      },
      templates: {
        actions: {
          addTranslation: 'Add translation',
          create: 'Create template',
          editTranslation: 'Edit translation',
        },
        channels: {
          configured: 'Configured',
          notConfigured: 'Not configured',
          notification: 'Notification',
        },
        fields: {
          actionLink: 'Action link',
          allowOptOut: 'Allow recipients to opt out',
        },
        formats: {
          html: 'HTML',
          text: 'Text',
        },
        messages: {
          deleteConfirm: 'Delete this template and all its translations?',
          deleteTitle: 'Delete template',
          deleteTranslationConfirm: 'Delete this language translation?',
          deleteTranslationTitle: 'Delete translation',
          empty: 'No templates found.',
          noTranslations: 'No translations found.',
        },
        name: RESOURCE_LABELS.courier.templates,
        notifications: {
          created: 'Template created.',
          deleted: 'Template deleted.',
          translationAdded: 'Translation added.',
          translationDeleted: 'Translation deleted.',
          translationUpdated: 'Translation updated.',
          updated: 'Template updated.',
        },
        pages: {
          create: 'Create template',
          edit: 'Edit template',
          list: RESOURCE_LABELS.courier.templates,
        },
        placeholders: {
          language: 'Select language',
        },
        sections: {
          translations: 'Translations',
        },
        types: {
          comment: 'Comment',
          email: 'Email',
          notification: 'Notification',
        },
        validation: {
          channelRequired: 'Enable email, notification, or both.',
          emailBody: 'Email body is required.',
          emailSubject: 'Email subject must contain at least 10 characters.',
          notificationMessage: 'Notification message is required.',
          notificationTitle: 'Notification title is required.',
        },
      },
    },
    iam: {
      authorization: {
        name: 'Authorization',
      },
      organizations: {
        messages: {
          deleteConfirm: 'Delete this organization?',
          empty: 'No organizations found.',
        },
        name: RESOURCE_LABELS.iam.organizations,
        notifications: {
          codeUpdated: 'Organization code updated.',
          deleted: 'Organization deleted.',
          updated: 'Organization updated.',
        },
        pages: {
          edit: 'Edit organization',
          list: RESOURCE_LABELS.iam.organizations,
          profile: RESOURCE_LABELS.iam.organizationProfile,
          show: 'Organization details',
        },
        placeholders: {
          search: 'Search organization',
        },
        types: ORGANIZATION_TYPE_LABELS,
      },
      parameters: {
        name: RESOURCE_LABELS.iam.parameters,
        messages: {
          empty: 'No parameters found.',
        },
        notifications: {
          overrideRemoved: 'Override removed.',
          overrideSaved: 'Override saved.',
          updated: 'Parameter updated.',
        },
        pages: {
          edit: 'Edit parameter',
          list: RESOURCE_LABELS.iam.parameters,
          organizationSettings: 'Settings',
          userSettings: 'Settings',
        },
      },
      permissions: {
        messages: {
          empty: 'No permissions found.',
        },
        name: RESOURCE_LABELS.iam.permissions,
        notifications: {
          updated: 'Permission updated.',
        },
        pages: {
          list: RESOURCE_LABELS.iam.permissions,
        },
      },
      roles: {
        labels: {
          assignedPermissions: 'Assigned permissions',
          availablePermissions: 'Available permissions',
        },
        messages: {
          deleteConfirm: 'Delete this role?',
          empty: 'No roles found.',
        },
        name: RESOURCE_LABELS.iam.roles,
        notifications: {
          created: 'Role created.',
          deleted: 'Role deleted.',
          permissionsAssigned: 'Permissions assigned.',
          permissionsUnassigned: 'Permissions removed.',
          updated: 'Role updated.',
        },
        pages: {
          create: 'Create role',
          edit: 'Edit role',
          list: RESOURCE_LABELS.iam.roles,
        },
      },
      users: {
        account: {
          communication: 'Communication',
          profile: 'Profile',
          security: 'Security',
          settings: 'Settings',
          title: 'User profile',
        },
        messages: {
          deleteConfirm: 'Delete this user?',
          empty: 'No users found.',
          noPermissions: 'No permissions found.',
          noRoles: 'No roles found.',
          organizationRequired: 'Select an organization.',
          passwordsDoNotMatch: 'Passwords do not match.',
        },
        name: RESOURCE_LABELS.iam.users,
        notifications: {
          created: 'User created.',
          deleted: 'User deleted.',
          passwordUpdated: 'Password updated.',
          profileUpdated: 'Profile updated.',
          updated: 'User updated.',
        },
        pages: {
          changePassword: 'Change password',
          create: 'Create user',
          edit: 'Edit user',
          list: RESOURCE_LABELS.iam.users,
          profile: RESOURCE_LABELS.iam.userProfile,
          show: 'User details',
        },
        placeholders: {
          search: 'Search user',
        },
        tabs: {
          permissions: 'Permissions',
          roles: 'Roles',
        },
      },
      userAccess: {
        labels: {
          assignedRoles: 'Assigned roles',
          availableRoles: 'Available roles',
        },
        messages: {
          noUserSelected: 'Select a user to manage access.',
          noRoles: 'No roles found.',
        },
        name: 'User Access',
        notifications: {
          rolesAssigned: 'Roles assigned.',
          rolesUnassigned: 'Roles removed.',
        },
        pages: {
          list: 'User Access',
        },
      },
    },
    sentinel: {
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
        levels: SYSTEM_LOG_LEVEL_LABELS,
        messages: {
          empty: 'No system logs found.',
        },
        name: RESOURCE_LABELS.sentinel.systemLogs,
        pages: {
          list: RESOURCE_LABELS.sentinel.systemLogs,
          show: 'System log details',
        },
        statuses: SYSTEM_LOG_STATUS_LABELS,
      },
    },
  },
  shared: {
    actions: {
      add: 'Add',
      back: 'Back',
      cancel: 'Cancel',
      clear: 'Clear',
      columns: 'Columns',
      create: 'Create',
      delete: 'Delete',
      edit: 'Edit',
      editCode: 'Edit code',
      filter: 'Filter',
      list: 'List',
      read: 'Read',
      remove: 'Remove',
      removeOverride: 'Remove override',
      reset: 'Reset',
      save: 'Save',
      saveOverride: 'Save override',
      update: 'Update',
      view: 'View',
      viewAccess: 'View access',
      viewPermissions: 'View permissions',
      write: 'Write',
    },
    common: {
      false: 'False',
      loading: 'Loading...',
      no: 'No',
      true: 'True',
      yes: 'Yes',
    },
    errors: {
      generic: 'Something went wrong.',
    },
    fields: {
      action: 'Action',
      actions: 'Actions',
      attemptedAt: 'Attempted at',
      assignedAt: 'Assigned at',
      assignedBy: 'Assigned by',
      code: 'Code',
      confirmPassword: 'Confirm password',
      createdAt: 'Created at',
      currentPassword: 'Current password',
      body: 'Body',
      dateFrom: 'Date from',
      dateTo: 'Date to',
      defaultLanguage: 'Default language',
      description: 'Description',
      email: 'Email',
      errorMessage: 'Error message',
      exception: 'Exception',
      expiresAt: 'Expires at',
      externalListEndpoint: 'External list endpoint',
      feature: 'Feature',
      format: 'Format',
      from: 'From',
      group: 'Group',
      id: 'ID',
      info: 'Info',
      ipAddress: 'IP address',
      isActive: 'Active',
      isDefault: 'Default',
      isOrganizationAdmin: 'Organization admin',
      isSystemAdmin: 'System admin',
      isVisible: 'Visible',
      isHtml: 'HTML',
      key: 'Key',
      language: 'Language',
      listItems: 'List items',
      level: 'Level',
      metadata: 'Metadata',
      message: 'Message',
      module: 'Module',
      name: 'Name',
      notification: 'Notification',
      newPassword: 'New password',
      nextAttemptAt: 'Next attempt at',
      organization: 'Organization',
      organizationId: 'Organization ID',
      overrideType: 'Override type',
      password: 'Password',
      privacyLevel: 'Privacy level',
      preview: 'Preview',
      propertiesJson: 'Properties JSON',
      resource: 'Resource',
      requestId: 'Request ID',
      recipient: 'Recipient',
      readAt: 'Read at',
      retentionPolicy: 'Retention policy',
      severity: 'Severity',
      retryCount: 'Retry count',
      startsAt: 'Starts at',
      stackTrace: 'Stack trace',
      status: 'Status',
      subject: 'Subject',
      targetId: 'Target ID',
      templateKey: 'Template key',
      title: 'Title',
      to: 'To',
      type: 'Type',
      user: 'User',
      userAgent: 'User agent',
      userId: 'User ID',
      sentAt: 'Sent at',
      value: 'Value',
    },
    languages: {
      en: 'English',
      es: 'Spanish',
      ptbr: 'Portuguese - Brazil',
    },
    filters: {
      all: 'All',
    },
    pagination: {
      pageSize: 'Rows per page',
      firstPage: 'First page',
      lastPage: 'Last page',
      nextPage: 'Next page',
      previousPage: 'Previous page',
      summary: 'Page {{page}} of {{pages}}',
      visibleRows: 'Showing {{start}}-{{end}} of {{total}}',
    },
    enums: {
      auditPrivacyLevel: AUDIT_PRIVACY_LEVEL_LABELS,
      emailStatus: EMAIL_STATUS_LABELS,
      notificationSeverity: NOTIFICATION_SEVERITY_LABELS,
      notificationStatus: NOTIFICATION_STATUS_LABELS,
      organizationType: ORGANIZATION_TYPE_LABELS,
      parameterOverrideType: PARAMETER_OVERRIDE_TYPE_LABELS,
      parameterType: PARAMETER_TYPE_LABELS,
      retentionPolicy: RETENTION_POLICY_LABELS,
      systemLogLevel: SYSTEM_LOG_LEVEL_LABELS,
      systemLogStatus: SYSTEM_LOG_STATUS_LABELS,
    },
    featureLabels: CORE_FEATURE_LABELS,
    status: {
      active: 'Active',
      inactive: 'Inactive',
    },
  },
  modules: MODULE_LABELS,
  resources: RESOURCE_LABELS,
} as const
