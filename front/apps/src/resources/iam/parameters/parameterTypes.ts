export const PARAMETER_QUERY_PARAMS = {
  group: 'Group',
  module: 'Module',
  name: 'Name',
  pageNumber: 'PageNumber',
  pageSize: 'PageSize',
  title: 'Title',
} as const

export const PARAMETER_FILTER_VALUES = {
  all: 'all',
} as const

export const PARAMETER_REQUEST_FIELDS = {
  description: 'Description',
  externalListEndpoint: 'ExternalListEndpoint',
  group: 'Group',
  isVisible: 'IsVisible',
  listItems: 'ListItems',
  module: 'Module',
  name: 'Name',
  overrideType: 'OverrideType',
  title: 'Title',
  type: 'Type',
  validationErrorCustomMessage: 'ValidationErrorCustomMessage',
  validationRegex: 'ValidationRegex',
  value: 'Value',
} as const

export const PARAMETER_OWNER_REQUEST_FIELDS = {
  value: 'Value',
} as const

export const PARAMETER_MODULE_OPTIONS = [
  { labelKey: 'modules.iam', value: 'IAM' },
  { labelKey: 'modules.courier', value: 'Courier' },
  { labelKey: 'modules.shared', value: 'Shared' },
] as const

export const PARAMETER_TYPE_OPTIONS = [
  { labelKey: 'common.enums.parameterType.boolean', value: '1' },
  { labelKey: 'common.enums.parameterType.integer', value: '2' },
  { labelKey: 'common.enums.parameterType.decimal', value: '3' },
  { labelKey: 'common.enums.parameterType.dateTime', value: '4' },
  { labelKey: 'common.enums.parameterType.date', value: '5' },
  { labelKey: 'common.enums.parameterType.time', value: '6' },
  { labelKey: 'common.enums.parameterType.character', value: '7' },
  { labelKey: 'common.enums.parameterType.string', value: '8' },
  { labelKey: 'common.enums.parameterType.text', value: '9' },
  { labelKey: 'common.enums.parameterType.richText', value: '10' },
  { labelKey: 'common.enums.parameterType.uuid', value: '11' },
  { labelKey: 'common.enums.parameterType.list', value: '12' },
  { labelKey: 'common.enums.parameterType.referenceId', value: '13' },
] as const

export const PARAMETER_OVERRIDE_TYPE_OPTIONS = [
  { labelKey: 'common.enums.parameterOverrideType.none', value: '0' },
  { labelKey: 'common.enums.parameterOverrideType.organizationId', value: '1' },
  { labelKey: 'common.enums.parameterOverrideType.userId', value: '2' },
] as const

export const PARAMETER_TYPE_VALUES = {
  boolean: '1',
  integer: '2',
  decimal: '3',
  dateTime: '4',
  date: '5',
  time: '6',
  character: '7',
  string: '8',
  text: '9',
  richText: '10',
  uuid: '11',
  list: '12',
  referenceId: '13',
} as const

export type ParameterLiteDto = {
  description: string
  id: string
  group: string
  isOverridden: boolean
  module: string
  name: string
  overrideType: number
  parameterOverrideId: string | null
  title: string
  type: number
  value: string
}

export type ParameterDto = ParameterLiteDto & {
  externalListEndpoint: string | null
  isVisible: boolean
  key: string
  listItems: string | null
}

export type ParameterSearchForm = {
  group: string
  module: string
  name: string
  title: string
}

export type ParameterListQuery = ParameterSearchForm & {
  pageNumber: number
  pageSize: number
}

export type ParameterForm = {
  description: string
  externalListEndpoint: string
  group: string
  isVisible: boolean
  listItems: string
  module: string
  name: string
  overrideType: string
  title: string
  type: string
  value: string
}

export type ParameterUpdateRequest = {
  Description: string
  ExternalListEndpoint: string | null
  Group: string
  IsVisible: boolean
  ListItems: string | null
  Module: string
  Name: string
  OverrideType: number
  Title: string
  Type: number
  ValidationErrorCustomMessage: string | null
  ValidationRegex: string | null
  Value: string
}

export type ParameterOwnerUpdateRequest = {
  Value: string
}
