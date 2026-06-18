import { ChevronDown, ChevronsUpDown, ChevronUp } from 'lucide-react'
import type { ReactNode } from 'react'

import { cn } from '../../lib/utils'

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
      className={cn('table-header-button', canSort && 'table-header-sortable')}
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
    return <ChevronUp />
  }

  if (direction === 'desc') {
    return <ChevronDown />
  }

  return <ChevronsUpDown />
}
