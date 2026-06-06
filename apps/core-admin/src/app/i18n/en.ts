import englishMessages from 'ra-language-english'

export const enMessages = {
  ...englishMessages,
  app: {
    dashboard: {
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
}
