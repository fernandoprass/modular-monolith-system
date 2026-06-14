export const API_PATHS = {
  iam: {
    authentication: {
      login: '/api/v1/iam/authentication/login',
    },
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
      permissionAssign: '/api/v1/iam/roles/permissions/assign',
      permissionUnassign: '/api/v1/iam/roles/permissions/unassign',
    },
    userAccess: {
      roleAssign: '/api/v1/iam/user-access/roles/assign',
      roleUnassign: '/api/v1/iam/user-access/roles/unassign',
      userPermissions: (userId: string) => `/api/v1/iam/user-access/users/${userId}/permissions`,
      userRoles: (userId: string) => `/api/v1/iam/user-access/users/${userId}/roles`,
    },
    users: {
      byId: (id: string | number) => `/api/v1/iam/users/${id}`,
      list: '/api/v1/iam/users',
      lookup: '/api/v1/iam/users/lookup',
      profile: '/api/v1/iam/users/profile',
    },
  },
} as const
