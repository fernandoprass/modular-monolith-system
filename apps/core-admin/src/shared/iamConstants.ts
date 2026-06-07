export const IAM_MODULE = 'iam'

export const IAM_RESOURCES = {
  authorization: 'authorization',
  organizations: 'organizations',
  parameters: 'parameters',
  permissions: 'permissions',
  roles: 'roles',
  users: 'users',
} as const

export const IAM_ACTIONS = {
  list: 'list',
  view: 'view',
} as const
