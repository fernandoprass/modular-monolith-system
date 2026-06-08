export const ORGANIZATION_TYPES = {
  company: 1,
  individual: 2,
} as const

export const ORGANIZATION_QUERY_PARAMS = {
  code: 'Code',
  name: 'Name',
  organizationId: 'OrganizationId',
  pageNumber: 'PageNumber',
  pageSize: 'PageSize',
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

export type OrganizationSearchRequest = {
  Type?: number
  Code?: string
  Name?: string
  OrganizationId?: string
  PageNumber: number
  PageSize: number
}

export type PagedResultDto<TItem> = {
  items: TItem[]
  pageNumber: number
  pageSize: number
  totalCount: number
  totalPages: number
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

export type OrganizationCreateForm = {
  type: number
  code: string
  name: string
  description: string
  defaultLanguage: string
  userName: string
  userEmail: string
  userPassword: string
}

export const organizationTypeChoices = [
  {
    id: ORGANIZATION_TYPES.company,
    name: 'resources.iam.organizations.types.company',
  },
  {
    id: ORGANIZATION_TYPES.individual,
    name: 'resources.iam.organizations.types.individual',
  },
]
