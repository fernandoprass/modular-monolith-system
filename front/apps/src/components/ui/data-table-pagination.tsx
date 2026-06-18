import { useTranslate } from '../../app/i18n/i18n'
import { PAGE_SIZE_OPTIONS } from '../../shared/pagination'
import { Button } from './button'
import { Select } from './select'

type DataTablePaginationProps = {
  onPageChange: (pageNumber: number) => void
  onPageSizeChange: (pageSize: number) => void
  pageNumber: number
  pageSize: number
  totalCount: number
  totalPages: number
}

function getDisplayPageNumber(pageNumber: number, totalPages: number) {
  if (totalPages < 1) {
    return 0
  }

  return Math.min(Math.max(pageNumber, 1), totalPages)
}

function getStartItemNumber(pageNumber: number, pageSize: number, totalCount: number) {
  if (pageNumber < 1 || totalCount < 1) {
    return 0
  }

  return Math.min(1 + (pageSize * (pageNumber - 1)), totalCount)
}

function getEndItemNumber(pageNumber: number, pageSize: number, totalCount: number) {
  if (pageNumber < 1 || totalCount < 1) {
    return 0
  }

  const end = pageSize * pageNumber

  return Math.min(end, totalCount)
}

export function DataTablePagination({
  onPageChange,
  onPageSizeChange,
  pageNumber,
  pageSize,
  totalCount,
  totalPages,
}: DataTablePaginationProps) {
  const t = useTranslate()
  const safeTotalPages = Math.max(totalPages, 0)
  const displayPageNumber = getDisplayPageNumber(pageNumber, safeTotalPages)
  const canGoPrevious = displayPageNumber > 1
  const canGoNext = displayPageNumber > 0 && displayPageNumber < safeTotalPages

  return (
    <div className="pagination-row">
      <div className="pagination-page-size">
        <span>{t('shared.pagination.pageSize')}</span>
        <Select
          onValueChange={(value) => onPageSizeChange(Number(value))}
          options={PAGE_SIZE_OPTIONS.map((option) => ({
            label: String(option),
            value: String(option),
          }))}
          value={String(pageSize)}
        />
      </div>
      <span>
        {t('shared.pagination.visibleRows', {
          start: getStartItemNumber(displayPageNumber, pageSize, totalCount),
          end: getEndItemNumber(displayPageNumber, pageSize, totalCount),
          total: totalCount,
        })}
      </span>
      <div className="pagination-actions">
        <span>{t('shared.pagination.summary', { page: displayPageNumber, pages: safeTotalPages })}</span>
        <Button disabled={!canGoPrevious} onClick={() => onPageChange(1)} type="button" variant="outline">
          {t('shared.actions.firstPage')}
        </Button>
        <Button disabled={!canGoPrevious} onClick={() => onPageChange(displayPageNumber - 1)} type="button" variant="outline">
          {t('shared.actions.previousPage')}
        </Button>
        <Button disabled={!canGoNext} onClick={() => onPageChange(displayPageNumber + 1)} type="button" variant="outline">
          {t('shared.actions.nextPage')}
        </Button>
        <Button disabled={!canGoNext} onClick={() => onPageChange(safeTotalPages)} type="button" variant="outline">
          {t('shared.actions.lastPage')}
        </Button>
      </div>
    </div>
  )
}
