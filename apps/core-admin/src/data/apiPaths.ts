export const API_PATHS = {
  iam: {
    users: {
      login: '/api/v1/iam/users/login',
    },
    roles: {
      userPermissions: (userId: string) => `/api/v1/iam/roles/user/${userId}/permissions`,
    },
    organizations: {
      byId: (id: string | number) => `/api/v1/iam/organizations/${id}`,
      code: (id: string | number) => `/api/v1/iam/organizations/${id}/code`,
      list: '/api/v1/iam/organizations',
    },
  },
} as const
