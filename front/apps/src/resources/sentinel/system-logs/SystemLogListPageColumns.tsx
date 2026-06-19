import type { ColumnDef } from '@tanstack/react-table'

import type { Translate } from '../../../app/i18n/i18n'
import {
  DATA_TABLE_ROW_ACTION_KINDS,
  DataTableRowActions,
} from '../../../components/ui/data-table-row-actions'
import type { SystemLogLiteDto } from './systemLogTypes'
import { getSystemLogLevelLabel, getSystemLogStatusLabel } from './systemLogUi'

type CreateSystemLogTableColumnsRequest = {
  onView: (systemLog: SystemLogLiteDto) => void
  t: Translate
}

export function createSystemLogTableColumns({
  onView,
  t,
}: CreateSystemLogTableColumnsRequest): ColumnDef<SystemLogLiteDto>[] {
  return [
    {
      accessorKey: 'createdAt',
      header: t('shared.fields.createdAt'),
    },
    {
      accessorKey: 'level',
      cell: ({ row }) => getSystemLogLevelLabel(row.original.level, t),
      header: t('shared.fields.level'),
    },
    {
      accessorKey: 'status',
      cell: ({ row }) => getSystemLogStatusLabel(row.original.status, t),
      header: t('shared.fields.status'),
    },
    {
      accessorKey: 'module',
      header: t('shared.fields.module'),
    },
    {
      accessorKey: 'message',
      header: t('shared.fields.message'),
    },
    {
      cell: ({ row }) => (
        <DataTableRowActions
          actions={[
            {
              kind: DATA_TABLE_ROW_ACTION_KINDS.view,
              label: t('shared.actions.view'),
              onClick: () => onView(row.original),
            },
          ]}
        />
      ),
      enableHiding: false,
      enableSorting: false,
      header: t('shared.fields.actions'),
      id: 'actions',
    },
  ]
}
