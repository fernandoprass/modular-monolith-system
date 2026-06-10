export const API_PATHS = {
  iam: {
    organizations: {
      byId: (id: string | number) => `/api/v1/iam/organizations/${id}`,
      code: (id: string | number) => `/api/v1/iam/organizations/${id}/code`,
      list: '/api/v1/iam/organizations',
      lookup: '/api/v1/iam/organizations/lookup',
      own: '/api/v1/iam/organizations/own',
    },
    roles: {
      userPermissions: (userId: string) => `/api/v1/iam/roles/user/${userId}/permissions`,
    },
    users: {
      byId: (id: string | number) => `/api/v1/iam/users/${id}`,
      list: '/api/v1/iam/users',
      login: '/api/v1/iam/users/login',
      me: '/api/v1/iam/users/me',
    },
  },
} as const
