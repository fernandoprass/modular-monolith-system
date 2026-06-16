import {
  flexRender,
  getCoreRowModel,
  getSortedRowModel,
  useReactTable,
  type ColumnDef,
  type OnChangeFn,
  type RowSelectionState,
  type SortingState,
} from '@tanstack/react-table'

import { DataTableSortableButton } from './data-table-sortable-button'
import { Empty, EmptyDescription } from './empty'
import { Skeleton } from './skeleton'

type DataTableProps<TData> = {
  columns: ColumnDef<TData>[]
  data: TData[]
  emptyText: string
  getRowId?: (row: TData) => string
  isLoading: boolean
  loadingText: string
  onRowSelectionChange?: OnChangeFn<RowSelectionState>
  onSortingChange: OnChangeFn<SortingState>
  rowSelection?: RowSelectionState
  sorting: SortingState
}

export function DataTable<TData>({
  columns,
  data,
  emptyText,
  getRowId,
  isLoading,
  loadingText,
  onRowSelectionChange,
  onSortingChange,
  rowSelection,
  sorting,
}: DataTableProps<TData>) {
  const tableColumns: ColumnDef<TData>[] = onRowSelectionChange === undefined
    ? columns
    : [
      {
        cell: ({ row }) => (
          <input
            aria-label="Select row"
            checked={row.getIsSelected()}
            className="checkbox"
            onChange={row.getToggleSelectedHandler()}
            type="checkbox"
          />
        ),
        enableSorting: false,
        header: ({ table }) => (
          <input
            aria-label="Select all rows"
            checked={table.getIsAllRowsSelected()}
            className="checkbox"
            onChange={table.getToggleAllRowsSelectedHandler()}
            type="checkbox"
          />
        ),
        id: 'select',
      },
      ...columns,
    ]

  const table = useReactTable({
    columns: tableColumns,
    data,
    enableRowSelection: onRowSelectionChange !== undefined,
    enableSortingRemoval: true,
    getCoreRowModel: getCoreRowModel(),
    getRowId,
    getSortedRowModel: getSortedRowModel(),
    onRowSelectionChange,
    onSortingChange,
    state: {
      rowSelection,
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
