export const DEFAULT_PAGINATION = {
  pageNumber: 1,
  pageSize: 25,
} as const

export const PAGE_SIZE_OPTIONS = [10, 25, 50] as const

export type PagedResultDto<TItem> = {
  items: TItem[]
  pageNumber: number
  pageSize: number
  totalCount: number
  totalPages: number
}
