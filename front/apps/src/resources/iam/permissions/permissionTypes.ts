export const PERMISSION_QUERY_PARAMS = {
  action: 'Action',
  includeInactive: 'IncludeInactive',
  isActive: 'IsActive',
  module: 'Module',
  pageNumber: 'PageNumber',
  pageSize: 'PageSize',
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
  { labelKey: 'modules.iam', value: 'iam' },
  { labelKey: 'modules.shared', value: 'shared' },
  { labelKey: 'modules.sentinel', value: 'sentinel' },
  { labelKey: 'modules.courier', value: 'courier' },
] as const

export const PERMISSION_RESOURCE_OPTIONS = [
  { labelKey: 'resources.iam.users', value: 'users' },
  { labelKey: 'resources.iam.parameters', value: 'parameters' },
  { labelKey: 'resources.iam.organizations', value: 'organizations' },
  { labelKey: 'resources.iam.organizationProfile', value: 'organizationprofile' },
  { labelKey: 'resources.iam.permissions', value: 'permissions' },
  { labelKey: 'resources.sentinel.auditLogs', value: 'auditlogs' },
  { labelKey: 'resources.sentinel.systemLogs', value: 'systemlogs' },
  { labelKey: 'resources.courier.emails', value: 'emails' },
  { labelKey: 'resources.courier.emailTemplates', value: 'emailtemplates' },
  { labelKey: 'resources.courier.templates', value: 'templates' },
  { labelKey: 'resources.iam.roles', value: 'roles' },
  { labelKey: 'resources.iam.userProfile', value: 'userprofile' },
] as const

export const PERMISSION_ACTION_OPTIONS = [
  { labelKey: 'common.actions.read', value: 'read' },
  { labelKey: 'common.actions.write', value: 'write' },
  { labelKey: 'common.actions.delete', value: 'delete' },
  { labelKey: 'common.actions.viewAccess', value: 'viewaccess' },
  { labelKey: 'common.actions.viewPermissions', value: 'viewpermissions' },
  { labelKey: 'common.actions.saveOverride', value: 'saveoverride' },
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
