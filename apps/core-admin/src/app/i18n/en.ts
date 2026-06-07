import englishMessages from 'ra-language-english'

export const enMessages = {
  ...englishMessages,
  app: {
    dashboard: {
      features: 'Available features',
      title: 'Core Admin',
      subtitle: 'Admin workspace for Core API modules.',
    },
  },
  auth: {
    login: {
      email: 'Email',
      password: 'Password',
      signUp: 'New here? Sign up',
      submit: 'Sign in',
    },
  },
  shared: {
    actions: {
      cancel: 'Cancel',
      save: 'Save',
    },
    languages: {
      en: 'English',
      es: 'Spanish',
      ptBr: 'Portuguese - Brazil',
    },
    notifications: {
      unsupportedDataProviderAction: 'This data action is not implemented yet.',
    },
  },
  navigation: {
    groups: {
      authorization: 'Authorization',
      iam: 'IAM',
    },
  },
  resources: {
    iam: {
      authorization: {
        name: 'Authorization',
      },
      organizations: {
        actions: {
          editCode: 'Edit code',
        },
        fields: {
          code: 'Code',
          defaultLanguage: 'Default language',
          description: 'Description',
          isActive: 'Active',
          name: 'Name',
          type: 'Type',
          userEmail: 'Admin email',
          userName: 'Admin name',
          userPassword: 'Admin password',
        },
        name: 'Organizations',
        notifications: {
          codeUpdated: 'Organization code updated.',
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
}
