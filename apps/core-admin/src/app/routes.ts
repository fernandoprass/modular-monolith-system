export const APP_ROUTES = {
  dashboard: '/',
  login: '/login',
  organizationEdit: (id: string) => `/organizations/${id}`,
  organizationShow: (id: string) => `/organizations/${id}/show`,
  organizations: '/organizations',
  parameters: '/parameters',
  permissions: '/permissions',
  registerOrganization: '/register-organization',
  roles: '/roles',
  users: '/users',
} as const
