import { ChevronLeft, ChevronRight, ChevronsLeft, ChevronsRight } from 'lucide-react'

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
        <Button
          aria-label={t('shared.pagination.firstPage')}
          disabled={!canGoPrevious}
          onClick={() => onPageChange(1)}
          size="icon"
          type="button"
          variant="outline"
        >
          <ChevronsLeft aria-hidden="true" />
        </Button>
        <Button
          aria-label={t('shared.pagination.previousPage')}
          disabled={!canGoPrevious}
          onClick={() => onPageChange(displayPageNumber - 1)}
          size="icon"
          type="button"
          variant="outline"
        >
          <ChevronLeft aria-hidden="true" />
        </Button>
        <Button
          aria-label={t('shared.pagination.nextPage')}
          disabled={!canGoNext}
          onClick={() => onPageChange(displayPageNumber + 1)}
          size="icon"
          type="button"
          variant="outline"
        >
          <ChevronRight aria-hidden="true" />
        </Button>
        <Button
          aria-label={t('shared.pagination.lastPage')}
          disabled={!canGoNext}
          onClick={() => onPageChange(safeTotalPages)}
          size="icon"
          type="button"
          variant="outline"
        >
          <ChevronsRight aria-hidden="true" />
        </Button>
      </div>
    </div>
  )
}
