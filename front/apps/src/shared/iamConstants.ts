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
  assign: 'assign',
  parameters: 'parameters',
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
    parameters: permission(IAM_RESOURCES.organizationprofile, IAM_ACTIONS.parameters),
    read: permission(IAM_RESOURCES.organizationprofile, IAM_ACTIONS.read),
    write: permission(IAM_RESOURCES.organizationprofile, IAM_ACTIONS.write),
  },
  parameters: {
    read: permission(IAM_RESOURCES.parameters, IAM_ACTIONS.read),
    write: permission(IAM_RESOURCES.parameters, IAM_ACTIONS.write),
  },
  permissions: {
    assign: permission(IAM_RESOURCES.permissions, IAM_ACTIONS.assign),
    read: permission(IAM_RESOURCES.permissions, IAM_ACTIONS.read),
    write: permission(IAM_RESOURCES.permissions, IAM_ACTIONS.write),
  },
  roles: {
    assign: permission(IAM_RESOURCES.roles, IAM_ACTIONS.assign),
    read: permission(IAM_RESOURCES.roles, IAM_ACTIONS.read),
    write: permission(IAM_RESOURCES.roles, IAM_ACTIONS.write),
  },
  users: {
    read: permission(IAM_RESOURCES.users, IAM_ACTIONS.read),
    write: permission(IAM_RESOURCES.users, IAM_ACTIONS.write),
  },
  userProfile: {
    delete: permission(IAM_RESOURCES.userprofile, 'delete'),
    parameters: permission(IAM_RESOURCES.userprofile, IAM_ACTIONS.parameters),
    read: permission(IAM_RESOURCES.userprofile, IAM_ACTIONS.read),
    viewAccess: permission(IAM_RESOURCES.userprofile, 'viewaccess'),
    write: permission(IAM_RESOURCES.userprofile, IAM_ACTIONS.write),
  },
} as const
