export const NOTIFICATION_QUERY_PARAMS = {
  dateFrom: 'DateFrom',
  dateTo: 'DateTo',
  module: 'Module',
  organizationId: 'OrganizationId',
  pageNumber: 'PageNumber',
  pageSize: 'PageSize',
  status: 'Status',
  title: 'Title',
  userId: 'UserId',
} as const

export const NOTIFICATION_FILTER_VALUES = {
  all: 'all',
} as const

export const NOTIFICATION_MODULE_OPTIONS = [
  { labelKey: 'modules.iam', value: 'iam' },
  { labelKey: 'modules.courier', value: 'courier' },
  { labelKey: 'modules.sentinel', value: 'sentinel' },
  { labelKey: 'modules.shared', value: 'shared' },
] as const

export const NOTIFICATION_STATUSES = {
  unread: 1,
  read: 2,
} as const

export type NotificationStatus = typeof NOTIFICATION_STATUSES[keyof typeof NOTIFICATION_STATUSES]

export type NotificationLiteDto = {
  actionLink: string
  createdAt: string
  feature: string
  id: string
  message: string
  module: string
  readAt: string | null
  status: NotificationStatus
  title: string
}

export type NotificationSearchForm = {
  dateFrom: string
  dateTo: string
  module: string
  status: string
  title: string
}

export type NotificationListQuery = NotificationSearchForm & {
  organizationId: string
  pageNumber: number
  pageSize: number
  userId: string
}
