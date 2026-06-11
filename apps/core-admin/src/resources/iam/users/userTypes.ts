export const USER_QUERY_PARAMS = {
  email: 'Email',
  id: 'Id',
  includeInactive: 'IncludeInactive',
  isActive: 'IsActive',
  name: 'Name',
  organizationId: 'OrganizationId',
  pageNumber: 'PageNumber',
  pageSize: 'PageSize',
  search: 'Search',
  take: 'Take',
} as const

export const USER_REQUEST_FIELDS = {
  email: 'Email',
  isActive: 'IsActive',
  language: 'Language',
  name: 'Name',
  organizationId: 'OrganizationId',
  password: 'Password',
} as const

export type UserLiteDto = {
  id: string
  name: string
  email: string
  isActive: boolean
  language: string
}

export type UserLookupDto = {
  id: string
  name: string
}

export type UserDto = UserLiteDto & {
  isSystemAdmin: boolean
  isOrganizationAdmin: boolean
  createdAt: string
  emailVerifiedAt: string | null
  lastLoginAt: string | null
  organizationId: string
  organizationName: string
}

export type UserRoleDto = {
  id: string
  roleId: string
  name: string
  isActive: boolean
  isDefault: boolean
  startsAt: string
  expiresAt: string | null
  assignedBy : string
  assignedAt: string
}

export type UserCreateForm = {
  email: string
  language: string
  name: string
  organizationId: string
  password: string
}

export type UserCreateRequest = {
  Name: string
  Email: string
  Password: string
  Language: string
  OrganizationId: string
}

export type UserUpdateRequest = {
  Name: string
  IsActive: boolean
  Language: string
}

export type PagedResultDto<TItem> = {
  items: TItem[]
  pageNumber: number
  pageSize: number
  totalCount: number
  totalPages: number
}
