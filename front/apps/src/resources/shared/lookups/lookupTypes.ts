import type { LanguageCode } from '../../../shared/languages'

export const LOOKUP_TYPE_ENUM = {
  country: 'country',
  state: 'state',
} as const

export type LookupType = typeof LOOKUP_TYPE_ENUM[keyof typeof LOOKUP_TYPE_ENUM]
export const LOOKUP_FILTER_VALUES = { all: 'all' } as const

export type LookupSearchForm = {
  code: string
  languageCode: LanguageCode
  type: LookupType | typeof LOOKUP_FILTER_VALUES.all
  value: string
}

export type LookupListQuery = LookupSearchForm & {
  pageNumber: number
  pageSize: number
}

export type LookupLiteDto = {
  id: string
  type: string
  code: string
  value: string
  description: string | null
  languageCode: string | null
  isActive: boolean
  parentId: string | null
  organizationId: string | null
}
