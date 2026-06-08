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

function permission(resource: string, action: string): string {
  return `${IAM_MODULE}.${resource}.${action}`
}

export const IAM_PERMISSIONS = {
  organizations: {
    create: permission(IAM_RESOURCES.organizations, IAM_ACTIONS.create),
    delete: permission(IAM_RESOURCES.organizations, IAM_ACTIONS.delete),
    list: permission(IAM_RESOURCES.organizations, IAM_ACTIONS.list),
    update: permission(IAM_RESOURCES.organizations, IAM_ACTIONS.update),
    view: permission(IAM_RESOURCES.organizations, IAM_ACTIONS.view),
  },
  parameters: {
    list: permission(IAM_RESOURCES.parameters, IAM_ACTIONS.list),
    view: permission(IAM_RESOURCES.parameters, IAM_ACTIONS.view),
  },
  permissions: {
    list: permission(IAM_RESOURCES.permissions, IAM_ACTIONS.list),
    view: permission(IAM_RESOURCES.permissions, IAM_ACTIONS.view),
  },
  roles: {
    list: permission(IAM_RESOURCES.roles, IAM_ACTIONS.list),
    view: permission(IAM_RESOURCES.roles, IAM_ACTIONS.view),
  },
  users: {
    list: permission(IAM_RESOURCES.users, IAM_ACTIONS.list),
    view: permission(IAM_RESOURCES.users, IAM_ACTIONS.view),
  },
} as const
