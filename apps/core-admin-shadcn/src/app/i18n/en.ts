export const enMessages = {
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
          show: 'Organization details',
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
        name: 'Permissions',
      },
      roles: {
        name: 'Roles',
      },
      users: {
        name: 'Users',
      },
    },
  },
  shared: {
    actions: {
      cancel: 'Cancel',
      close: 'Close',
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
    pagination: {
      summary: 'Page {{page}} of {{pages}}. {{total}} total.',
    },
    status: {
      active: 'Active',
      inactive: 'Inactive',
    },
  },
} as const
