export const USER_QUERY_PARAMS = {
  email: 'Email',
  name: 'Name',
  organizationId: 'OrganizationId',
  pageNumber: 'PageNumber',
  pageSize: 'PageSize',
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

export type UserDto = UserLiteDto & {
  isSystemAdmin: boolean
  isOrganizationAdmin: boolean
  createdAt: string
  emailVerifiedAt: string | null
  lastLoginAt: string | null
  organizationId: string
  organizationName: string
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
