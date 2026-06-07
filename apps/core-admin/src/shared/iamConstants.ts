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
  create: 'create',
  delete: 'delete',
  list: 'list',
  update: 'update',
  view: 'view',
} as const

export const IAM_PERMISSIONS = {
  organizations: {
    create: `${IAM_MODULE}.${IAM_RESOURCES.organizations}.${IAM_ACTIONS.create}`,
    delete: `${IAM_MODULE}.${IAM_RESOURCES.organizations}.${IAM_ACTIONS.delete}`,
    update: `${IAM_MODULE}.${IAM_RESOURCES.organizations}.${IAM_ACTIONS.update}`,
    view: `${IAM_MODULE}.${IAM_RESOURCES.organizations}.${IAM_ACTIONS.view}`,
  },
} as const
