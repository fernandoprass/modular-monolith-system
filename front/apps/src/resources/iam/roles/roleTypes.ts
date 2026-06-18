export const ROLE_QUERY_PARAMS = {
  isActive: 'IsActive',
  name: 'Name',
  organizationId: 'OrganizationId',
  userId: 'UserId',
} as const

export const ROLE_REQUEST_FIELDS = {
  description: 'Description',
  isActive: 'IsActive',
  isDefault: 'IsDefault',
  name: 'Name',
  organizationId: 'OrganizationId',
} as const

export type RoleDto = {
  id: string
  name: string
  description: string
  isActive: boolean
  isDefault: boolean
  organizationId: string | null
}

export type RoleSearchForm = {
  name: string
  organizationId: string
  userId: string
}

export type RoleForm = {
  description: string
  isActive: boolean
  isDefault: boolean
  name: string
  organizationId: string
}

export type RoleCreateRequest = {
  Name: string
  Description: string
  IsDefault: boolean
  IsActive: boolean
  OrganizationId: string | null
}

export type RoleUpdateRequest = {
  Name: string
  Description: string
  IsDefault: boolean
  IsActive: boolean
}
