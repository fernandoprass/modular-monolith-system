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
  resources: {
    iam: {
      authorization: {
        name: 'Authorization',
      },
      organizations: {
        actions: {
          delete: 'Delete',
          edit: 'Edit',
          editCode: 'Edit code',
          filter: 'Filter',
          reset: 'Reset',
          view: 'View',
        },
        fields: {
          actions: 'Actions',
          code: 'Code',
          defaultLanguage: 'Default language',
          description: 'Description',
          isActive: 'Active',
          name: 'Name',
          type: 'Type',
        },
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
        actions: {
          create: 'Create',
          delete: 'Delete',
          edit: 'Edit permission',
          list: 'List',
          read: 'Read',
          saveoverride: 'Save override',
          update: 'Update',
          view: 'View',
          viewaccess: 'View access',
          viewpermissions: 'View permissions',
          write: 'Write',
        },
        fields: {
          action: 'Action',
          actions: 'Actions',
          description: 'Description',
          isActive: 'Active',
          module: 'Module',
          resource: 'Resource',
          title: 'Title',
        },
        messages: {
          empty: 'No permissions found.',
        },
        modules: {
          courier: 'Courier',
          iam: 'IAM',
          sentinel: 'Sentinel',
          shared: 'Shared',
        },
        name: 'Permissions',
        notifications: {
          updated: 'Permission updated.',
        },
        pages: {
          list: 'Permissions',
        },
        resources: {
          auditlogs: 'Audit logs',
          emails: 'Emails',
          emailtemplates: 'Email templates',
          organizationprofile: 'Organization profile',
          organizations: 'Organizations',
          parameters: 'Parameters',
          permissions: 'Permissions',
          roles: 'Roles',
          systemlogs: 'System logs',
          templates: 'Templates',
          userprofile: 'User profile',
          users: 'Users',
        },
      },
      roles: {
        actions: {
          create: 'Create role',
          delete: 'Delete',
          edit: 'Edit role',
        },
        fields: {
          actions: 'Actions',
          assignedBy: 'Assigned by',
          assignedAt: 'Assigned at',
          description: 'Description',
          isActive: 'Active',
          isDefault: 'Default',
          name: 'Name',
          organizationId: 'Organization',
          startsAt: 'Starts at',
          expiresAt: 'Expires at',
          userId: 'User ID',
        },
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
        actions: {
          create: 'Create user',
          delete: 'Delete',
          edit: 'Edit',
          view: 'View',
        },
        fields: {
          actions: 'Actions',
          email: 'Email',
          isActive: 'Active',
          isOrganizationAdmin: 'Organization admin',
          isSystemAdmin: 'System admin',
          language: 'Language',
          name: 'Name',
          organizationId: 'Organization ID',
          organizationName: 'Organization',
          password: 'Password',
        },
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
      cancel: 'Cancel',
      back: 'Back',
      clear: 'Clear',
      columns: 'Columns',
      filter: 'Filter',
      firstPage: '<<', 
      nextPage: '>',
      previousPage: '<',
      lastPage: '>>', 
      reset: 'Reset',
      save: 'Save',
    },
    common: {
      loading: 'Loading...',
      no: 'No',
      yes: 'Yes',
    },
    errors: {
      generic: 'Something went wrong.',
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
  resources2: {
    courier: {
      email: 'Email',
      templates: 'Templates',       
    },
    sentinel: {
      auditlogs: 'Audit logs',
      systemlogs: 'System logs',
    },
    iam : {
      organization: 'Organization',
      organizations: 'Organizations',
      parameter: 'Parameter',
      parameters: 'Parameters',
      permission: 'Permission',
      permissions: 'Permissions',
      role: 'Role',
      roles: 'Roles'
    }
  }
} as const
