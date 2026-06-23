export const TEMPLATE_QUERY_PARAMS = {
  key: 'Key',
  name: 'Name',
  pageNumber: 'PageNumber',
  pageSize: 'PageSize',
  type: 'Type',
} as const

export const TEMPLATE_REQUEST_FIELDS = {
  key: 'Key',
  name: 'Name',
  retentionPolicy: 'RetentionPolicy',
  type: 'Type',
} as const

export const TEMPLATE_TRANSLATION_REQUEST_FIELDS = {
  body: 'Body',
  language: 'Language',
  subject: 'Subject',
} as const

export const TEMPLATE_FILTER_VALUES = {
  all: 'all',
} as const

export const TEMPLATE_TYPES = {
  comment: 1,
  email: 2,
  notification: 3,
} as const

export const TEMPLATE_TYPE_OPTIONS = [
  { labelKey: 'features.courier.templates.types.comment', value: String(TEMPLATE_TYPES.comment) },
  { labelKey: 'features.courier.templates.types.email', value: String(TEMPLATE_TYPES.email) },
  { labelKey: 'features.courier.templates.types.notification', value: String(TEMPLATE_TYPES.notification) },
] as const

export const RETENTION_POLICIES = {
  operational: 1,
  standard: 2,
  extended: 3,
  compliance: 4,
  longTerm: 5,
} as const

export const RETENTION_POLICY_OPTIONS = [
  { labelKey: 'features.courier.templates.retentionPolicies.operational', value: String(RETENTION_POLICIES.operational) },
  { labelKey: 'features.courier.templates.retentionPolicies.standard', value: String(RETENTION_POLICIES.standard) },
  { labelKey: 'features.courier.templates.retentionPolicies.extended', value: String(RETENTION_POLICIES.extended) },
  { labelKey: 'features.courier.templates.retentionPolicies.compliance', value: String(RETENTION_POLICIES.compliance) },
  { labelKey: 'features.courier.templates.retentionPolicies.longTerm', value: String(RETENTION_POLICIES.longTerm) },
] as const

export type TemplateType = typeof TEMPLATE_TYPES[keyof typeof TEMPLATE_TYPES]
export type RetentionPolicy = typeof RETENTION_POLICIES[keyof typeof RETENTION_POLICIES]

export type TemplateLiteDto = {
  id: string
  key: string
  name: string
  retentionPolicy: RetentionPolicy
  type: TemplateType
}

export type TemplateEmailTranslationDto = {
  body: string
  isHtml: boolean
  language: string
  subject: string
}

export type TemplateDto = TemplateLiteDto & {
  createdAt: string
  createdBy: string
  emailTranslations: TemplateEmailTranslationDto[]
  updatedAt: string | null
  updatedBy: string | null
}

export type TemplateSearchForm = {
  key: string
  name: string
  type: string
}

export type TemplateListQuery = TemplateSearchForm & {
  pageNumber: number
  pageSize: number
}

export type TemplateForm = {
  key: string
  name: string
  retentionPolicy: string
  type: string
}

export type TemplateRequest = {
  Key: string
  Name: string
  RetentionPolicy: number
  Type: number
}

export type TemplateTranslationForm = {
  body: string
  language: string
  subject: string
}

export type TemplateTranslationRequest = {
  Body: string
  Language: string
  Subject: string
}
