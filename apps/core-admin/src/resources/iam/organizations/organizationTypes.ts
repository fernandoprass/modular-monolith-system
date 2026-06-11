export const ORGANIZATION_TYPES = {
  company: 1,
  individual: 2,
} as const

export const ORGANIZATION_QUERY_PARAMS = {
  code: 'Code',
  id: 'Id',
  includeInactive: 'IncludeInactive',
  isActive: 'IsActive',
  name: 'Name',
  organizationId: 'OrganizationId',
  pageNumber: 'PageNumber',
  pageSize: 'PageSize',
  search: 'Search',
  take: 'Take',
  type: 'Type',
} as const

export const ORGANIZATION_REQUEST_FIELDS = {
  code: 'Code',
  defaultLanguage: 'DefaultLanguage',
  description: 'Description',
  isActive: 'IsActive',
  name: 'Name',
  type: 'Type',
  user: 'User',
  userEmail: 'Email',
  userName: 'Name',
  userPassword: 'Password',
} as const

export type OrganizationDto = {
  id: string
  type: number
  code: string
  name: string
  description: string | null
  defaultLanguage: string
  isActive: boolean
}

export type OrganizationLookupDto = {
  id: string
  code: string
  name: string
  isActive: boolean
}

export type OrganizationCreateForm = {
  code: string
  defaultLanguage: string
  description: string
  name: string
  type: number
  userEmail: string
  userName: string
  userPassword: string
}

export type OrganizationCreateRequest = {
  Type: number
  Name: string
  Code: string
  Description: string
  DefaultLanguage: string
  User: {
    Name: string
    Email: string
    Password: string
  }
}

export type OrganizationUpdateRequest = {
  Name: string
  Description: string
  IsActive: boolean
  DefaultLanguage: string
}

export type OrganizationCodeUpdateRequest = {
  Code: string
}

export type PagedResultDto<TItem> = {
  items: TItem[]
  pageNumber: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export const ORGANIZATION_TYPE_OPTIONS = [
  {
    labelKey: 'resources.iam.organizations.types.company',
    value: String(ORGANIZATION_TYPES.company),
  },
  {
    labelKey: 'resources.iam.organizations.types.individual',
    value: String(ORGANIZATION_TYPES.individual),
  },
] as const
