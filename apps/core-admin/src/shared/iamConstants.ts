export const IAM_MODULE = 'iam'

export const IAM_RESOURCES = {
  authorization: 'authorization',
  organizationprofile: 'organizationprofile',
  organizations: 'organizations',
  parameters: 'parameters',
  permissions: 'permissions',
  roles: 'roles',
  userprofile: 'userprofile',
  users: 'users',
} as const

export const IAM_ACTIONS = {
  read: 'read',
  write: 'write',
} as const

function permission(resource: string, action: string): string {
  return `${IAM_MODULE}.${resource}.${action}`
}

export const IAM_PERMISSIONS = {
  organizations: {
    read: permission(IAM_RESOURCES.organizations, IAM_ACTIONS.read),
    write: permission(IAM_RESOURCES.organizations, IAM_ACTIONS.write),
  },
  organizationProfile: {
    delete: permission(IAM_RESOURCES.organizationprofile, 'delete'),
    read: permission(IAM_RESOURCES.organizationprofile, IAM_ACTIONS.read),
    write: permission(IAM_RESOURCES.organizationprofile, IAM_ACTIONS.write),
  },
  parameters: {
    read: permission(IAM_RESOURCES.parameters, IAM_ACTIONS.read),
    write: permission(IAM_RESOURCES.parameters, IAM_ACTIONS.write),
  },
  permissions: {
    read: permission(IAM_RESOURCES.permissions, IAM_ACTIONS.read),
    write: permission(IAM_RESOURCES.permissions, IAM_ACTIONS.write),
  },
  roles: {
    read: permission(IAM_RESOURCES.roles, IAM_ACTIONS.read),
    write: permission(IAM_RESOURCES.roles, IAM_ACTIONS.write),
  },
  users: {
    read: permission(IAM_RESOURCES.users, IAM_ACTIONS.read),
    write: permission(IAM_RESOURCES.users, IAM_ACTIONS.write),
  },
  userProfile: {
    delete: permission(IAM_RESOURCES.userprofile, 'delete'),
    read: permission(IAM_RESOURCES.userprofile, IAM_ACTIONS.read),
    viewAccess: permission(IAM_RESOURCES.userprofile, 'viewaccess'),
    write: permission(IAM_RESOURCES.userprofile, IAM_ACTIONS.write),
  },
} as const
