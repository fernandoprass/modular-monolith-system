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
        messages: {
          deleteConfirm: 'Delete this role?',
          empty: 'No roles found.',
        },
        name: 'Roles',
        notifications: {
          created: 'Role created.',
          deleted: 'Role deleted.',
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
        },
        name: 'Users',
        notifications: {
          created: 'User created.',
          deleted: 'User deleted.',
          profileUpdated: 'Profile updated.',
          updated: 'User updated.',
        },
        pages: {
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
      loading: 'Loading...',
      no: 'No',
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
      defaultLanguage: 'Default language',
      description: 'Description',
      email: 'Email',
      expiresAt: 'Expires at',
      isActive: 'Active',
      isDefault: 'Default',
      isOrganizationAdmin: 'Organization admin',
      isSystemAdmin: 'System admin',
      language: 'Language',
      module: 'Module',
      name: 'Name',
      organization: 'Organization',
      organizationId: 'Organization ID',
      password: 'Password',
      resource: 'Resource',
      startsAt: 'Starts at',
      title: 'Title',
      type: 'Type',
      user: 'User',
      userId: 'User ID',
    },
    languages: {
      en: 'English',
      es: 'Spanish',
      ptBr: 'Portuguese - Brazil',
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
