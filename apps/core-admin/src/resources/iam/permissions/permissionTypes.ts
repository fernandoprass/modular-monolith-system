export const PERMISSION_QUERY_PARAMS = {
  action: 'Action',
  includeInactive: 'IncludeInactive',
  isActive: 'IsActive',
  module: 'Module',
  resource: 'Resource',
  title: 'Title',
} as const

export const PERMISSION_REQUEST_FIELDS = {
  action: 'Action',
  description: 'Description',
  isActive: 'IsActive',
  module: 'Module',
  resource: 'Resource',
  title: 'Title',
} as const

export const PERMISSION_FILTER_VALUES = {
  all: 'all',
  active: 'true',
  inactive: 'false',
} as const

export const PERMISSION_MODULE_OPTIONS = [
  { labelKey: 'resources.iam.permissions.modules.iam', value: 'iam' },
  { labelKey: 'resources.iam.permissions.modules.shared', value: 'shared' },
  { labelKey: 'resources.iam.permissions.modules.sentinel', value: 'sentinel' },
  { labelKey: 'resources.iam.permissions.modules.courier', value: 'courier' },
] as const

export const PERMISSION_RESOURCE_OPTIONS = [
  { labelKey: 'resources.iam.permissions.resources.users', value: 'users' },
  { labelKey: 'resources.iam.permissions.resources.parameters', value: 'parameters' },
  { labelKey: 'resources.iam.permissions.resources.organizations', value: 'organizations' },
  { labelKey: 'resources.iam.permissions.resources.permissions', value: 'permissions' },
  { labelKey: 'resources.iam.permissions.resources.auditlogs', value: 'auditlogs' },
  { labelKey: 'resources.iam.permissions.resources.systemlogs', value: 'systemlogs' },
  { labelKey: 'resources.iam.permissions.resources.emails', value: 'emails' },
  { labelKey: 'resources.iam.permissions.resources.emailtemplates', value: 'emailtemplates' },
  { labelKey: 'resources.iam.permissions.resources.templates', value: 'templates' },
  { labelKey: 'resources.iam.permissions.resources.roles', value: 'roles' },
] as const

export const PERMISSION_ACTION_OPTIONS = [
  { labelKey: 'resources.iam.permissions.actions.list', value: 'list' },
  { labelKey: 'resources.iam.permissions.actions.create', value: 'create' },
  { labelKey: 'resources.iam.permissions.actions.view', value: 'view' },
  { labelKey: 'resources.iam.permissions.actions.update', value: 'update' },
  { labelKey: 'resources.iam.permissions.actions.delete', value: 'delete' },
  { labelKey: 'resources.iam.permissions.actions.viewpermissions', value: 'viewpermissions' },
  { labelKey: 'resources.iam.permissions.actions.saveoverride', value: 'saveoverride' },
  { labelKey: 'resources.iam.permissions.actions.updatepassword', value: 'updatepassword' },
] as const

export type PermissionSearchForm = {
  action: string
  isActive: string
  module: string
  resource: string
  title: string
}

export type PermissionUpdateForm = {
  action: string
  description: string
  isActive: boolean
  module: string
  resource: string
  title: string
}

export type PermissionUpdateRequest = {
  Module: string
  Resource: string
  Action: string
  Title: string
  Description: string
  IsActive: boolean
}
