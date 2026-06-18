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
      iam: 'IAM',
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
    iam: {
      authorization: {
        name: 'Authorization',
      },
      organizations: {
        messages: {
          deleteConfirm: 'Delete this organization?',
          empty: 'No organizations found.',
        },
        name: 'Organizations',
        notifications: {
          codeUpdated: 'Organization code updated.',
          deleted: 'Organization deleted.',
          updated: 'Organization updated.',
        },
        pages: {
          edit: 'Edit organization',
          list: 'Organizations',
          profile: 'Organization profile',
          show: 'Organization details',
        },
        placeholders: {
          search: 'Search organization',
        },
        types: {
          company: 'Company',
          individual: 'Individual',
        },
      },
      parameters: {
        name: 'Parameters',
        messages: {
          empty: 'No parameters found.',
        },
        notifications: {
          updated: 'Parameter updated.',
        },
        pages: {
          edit: 'Edit parameter',
          list: 'Parameters',
          organizationSettings: 'Settings',
          userSettings: 'Settings',
        },
      },
      permissions: {
        messages: {
          empty: 'No permissions found.',
        },
        name: 'Permissions',
        notifications: {
          updated: 'Permission updated.',
        },
        pages: {
          list: 'Permissions',
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
        name: 'Roles',
        notifications: {
          created: 'Role created.',
          deleted: 'Role deleted.',
          permissionsAssigned: 'Permissions assigned.',
          permissionsUnassigned: 'Permissions removed.',
          updated: 'Role updated.',
        },
        pages: {
          list: 'Roles',
        },
      },
      users: {
        messages: {
          deleteConfirm: 'Delete this user?',
          empty: 'No users found.',
          noPermissions: 'No permissions found.',
          noRoles: 'No roles found.',
          organizationRequired: 'Select an organization.',
          passwordsDoNotMatch: 'Passwords do not match.',
        },
        name: 'Users',
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
          list: 'Users',
          profile: 'User profile',
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
  },
  shared: {
    actions: {
      back: 'Back',
      cancel: 'Cancel',
      clear: 'Clear',
      columns: 'Columns',
      create: 'Create',
      delete: 'Delete',
      edit: 'Edit',
      editCode: 'Edit code',
      filter: 'Filter',
      firstPage: '<<', 
      lastPage: '>>', 
      list: 'List',
      nextPage: '>',
      previousPage: '<',
      read: 'Read',
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
      assignedAt: 'Assigned at',
      assignedBy: 'Assigned by',
      code: 'Code',
      confirmPassword: 'Confirm password',
      currentPassword: 'Current password',
      defaultLanguage: 'Default language',
      description: 'Description',
      email: 'Email',
      expiresAt: 'Expires at',
      externalListEndpoint: 'External list endpoint',
      group: 'Group',
      info: 'Info',
      isActive: 'Active',
      isDefault: 'Default',
      isOrganizationAdmin: 'Organization admin',
      isSystemAdmin: 'System admin',
      isVisible: 'Visible',
      key: 'Key',
      language: 'Language',
      listItems: 'List items',
      module: 'Module',
      name: 'Name',
      newPassword: 'New password',
      organization: 'Organization',
      organizationId: 'Organization ID',
      overrideType: 'Override type',
      password: 'Password',
      resource: 'Resource',
      startsAt: 'Starts at',
      title: 'Title',
      type: 'Type',
      user: 'User',
      userId: 'User ID',
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
      summary: 'Page {{page}} of {{pages}}',
      visibleRows: 'Showing {{start}}-{{end}} of {{total}}',
    },
    status: {
      active: 'Active',
      inactive: 'Inactive',
    },
  },
  modules: {
    courier:  'Courier',
    iam:  'IAM',
    sentinel: 'Sentinel',
    shared:  'Shared',
  },
  resources: {
    courier: {
      emailtemplates: 'Email templates',
      emails: 'Emails',
      templates: 'Templates',       
    },
    sentinel: {
      auditlogs: 'Audit logs',
      systemlogs: 'System logs',
    },
    iam : {
      organization: 'Organization',
      organizationprofile: 'Organization profile',
      organizations: 'Organizations',
      parameter: 'Parameter',
      parameterOverrideTypes: {
        none: 'None',
        organizationId: 'Organization',
        userId: 'User',
      },
      parameterTypes: {
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
      },
      parameters: 'Parameters',
      permission: 'Permission',
      permissions: 'Permissions',
      role: 'Role',
      roles: 'Roles',
      userprofile: 'User profile',
      users: 'Users',
    }
  }
} as const
