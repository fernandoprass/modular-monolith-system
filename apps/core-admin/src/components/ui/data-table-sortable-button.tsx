import { ChevronDown, ChevronsUpDown, ChevronUp } from 'lucide-react'
import type { ReactNode } from 'react'

type DataTableSortDirection = false | 'asc' | 'desc'

type DataTableSortableButtonProps = {
  canSort: boolean
  children: ReactNode
  direction: DataTableSortDirection
  onClick?: (event: unknown) => void
}

export function DataTableSortableButton({
  canSort,
  children,
  direction,
  onClick,
}: DataTableSortableButtonProps) {
  return (
    <button
      className={`table-header-button ${canSort ? 'table-header-sortable' : ''}`}
      disabled={!canSort}
      onClick={onClick}
      type="button"
    >
      {children}
      {canSort && <SortIcon direction={direction} />}
    </button>
  )
}

type SortIconProps = {
  direction: DataTableSortDirection
}

function SortIcon({ direction }: SortIconProps) {
  if (direction === 'asc') {
    return <ChevronUp size={14} />
  }

  if (direction === 'desc') {
    return <ChevronDown size={14} />
  }

  return <ChevronsUpDown size={14} />
}
