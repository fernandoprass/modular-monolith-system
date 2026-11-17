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
  { labelKey: 'common.labels.users', value: 'users' },
  { labelKey: 'features.iam.labels.parameters', value: 'parameters' },
  { labelKey: 'features.iam.labels.organizations', value: 'organizations' },
  { labelKey: 'features.iam.labels.organizationProfile', value: 'organizationprofile' },
  { labelKey: 'features.iam.labels.permissions', value: 'permissions' },
  { labelKey: 'features.sentinel.labels.auditLogs', value: 'auditlogs' },
  { labelKey: 'features.sentinel.labels.systemLogs', value: 'systemlogs' },
  { labelKey: 'common.labels.emails', value: 'emails' },
  { labelKey: 'features.courier.labels.emailTemplates', value: 'emailtemplates' },
  { labelKey: 'features.courier.labels.templates', value: 'templates' },
  { labelKey: 'features.iam.labels.roles', value: 'roles' },
  { labelKey: 'features.iam.labels.userProfile', value: 'userprofile' },
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
