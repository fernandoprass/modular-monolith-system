export const API_PATHS = {
  iam: {
    users: {
      login: '/api/v1/iam/users/login',
    },
    roles: {
      userPermissions: (userId: string) => `/api/v1/iam/roles/user/${userId}/permissions`,
    },
  },
} as const
