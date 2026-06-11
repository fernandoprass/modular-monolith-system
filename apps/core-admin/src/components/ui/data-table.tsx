import {
  flexRender,
  getCoreRowModel,
  getSortedRowModel,
  useReactTable,
  type ColumnDef,
  type OnChangeFn,
  type SortingState,
} from '@tanstack/react-table'

import { DataTableSortableButton } from './data-table-sortable-button'
import { Empty, EmptyDescription } from './empty'
import { Skeleton } from './skeleton'

type DataTableProps<TData> = {
  columns: ColumnDef<TData>[]
  data: TData[]
  emptyText: string
  isLoading: boolean
  loadingText: string
  onSortingChange: OnChangeFn<SortingState>
  sorting: SortingState
}

export function DataTable<TData>({
  columns,
  data,
  emptyText,
  isLoading,
  loadingText,
  onSortingChange,
  sorting,
}: DataTableProps<TData>) {
  const table = useReactTable({
    columns,
    data,
    enableSortingRemoval: true,
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(),
    onSortingChange,
    state: {
      sorting,
    },
  })

  return (
    <div className="table-panel">
      <table className="data-table">
        <thead>
          {table.getHeaderGroups().map((headerGroup) => (
            <tr key={headerGroup.id}>
              {headerGroup.headers.map((header) => (
                <th className={header.column.id === 'actions' ? 'actions-column' : undefined} key={header.id}>
                  {header.isPlaceholder
                    ? null
                    : (
                      <DataTableSortableButton
                        canSort={header.column.getCanSort()}
                        direction={header.column.getIsSorted()}
                        onClick={header.column.getToggleSortingHandler()}
                      >
                        {flexRender(header.column.columnDef.header, header.getContext())}
                      </DataTableSortableButton>
                    )}
                </th>
              ))}
            </tr>
          ))}
        </thead>
        <tbody>
          {table.getRowModel().rows.map((row) => (
            <tr key={row.id}>
              {row.getVisibleCells().map((cell) => (
                <td key={cell.id}>
                  {flexRender(cell.column.columnDef.cell, cell.getContext())}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
      {!isLoading && data.length === 0 && (
        <Empty>
          <EmptyDescription>{emptyText}</EmptyDescription>
        </Empty>
      )}
      {isLoading && (
        <div aria-label={loadingText} className="table-skeleton">
          <Skeleton />
          <Skeleton />
          <Skeleton />
        </div>
      )}
    </div>
  )
}
