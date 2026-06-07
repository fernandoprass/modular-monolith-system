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
      submit: 'Sign in',
    },
  },
  shared: {
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
        name: 'Organizations',
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
