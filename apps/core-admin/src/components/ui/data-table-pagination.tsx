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

function getStartItemNumber(pageNumber: number, pageSize: number) {
  return 1 + (pageSize * (pageNumber - 1))
}

function getEndItemNumber(pageNumber: number, pageSize: number, totalCount: number) {
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
          start: getStartItemNumber(pageNumber, pageSize),
          end: getEndItemNumber(pageNumber, pageSize, totalCount),
          total: totalCount,
        })}
      </span>
      <div className="pagination-actions">
        <span>{t('shared.pagination.summary', { page: pageNumber, pages: totalPages })}</span>
        <Button disabled={pageNumber <= 1} onClick={() => onPageChange(1)} type="button" variant="outline">
          {t('shared.actions.firstPage')}
        </Button>
        <Button disabled={pageNumber <= 1} onClick={() => onPageChange(pageNumber - 1)} type="button" variant="outline">
          {t('shared.actions.previousPage')}
        </Button>
        <Button disabled={pageNumber >= totalPages} onClick={() => onPageChange(pageNumber + 1)} type="button" variant="outline">
          {t('shared.actions.nextPage')}
        </Button>
        <Button disabled={pageNumber >= totalPages} onClick={() => onPageChange(totalPages)} type="button" variant="outline">
          {t('shared.actions.lastPage')}
        </Button>
      </div>
    </div>
  )
}
