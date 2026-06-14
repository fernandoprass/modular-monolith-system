export const API_PATHS = {
  iam: {
    organizations: {
      byId: (id: string | number) => `/api/v1/iam/organizations/${id}`,
      code: (id: string | number) => `/api/v1/iam/organizations/${id}/code`,
      list: '/api/v1/iam/organizations',
      lookup: '/api/v1/iam/organizations/lookup',
      profile: '/api/v1/iam/organizations/profile',
    },
    permissions: {
      byId: (id: string | number) => `/api/v1/iam/permissions/${id}`,
      list: '/api/v1/iam/permissions',
    },
    roles: {
      byId: (id: string | number) => `/api/v1/iam/roles/${id}`,
      list: '/api/v1/iam/roles',
      userPermissions: (userId: string) => `/api/v1/iam/roles/user/${userId}/permissions`,
      userRoles: (userId: string) => `/api/v1/iam/roles/user/${userId}/roles`,
    },
    users: {
      byId: (id: string | number) => `/api/v1/iam/users/${id}`,
      list: '/api/v1/iam/users',
      login: '/api/v1/iam/users/login',
      lookup: '/api/v1/iam/users/lookup',
      profile: '/api/v1/iam/users/profile',
    },
  },
} as const
