import { formatUserDateTime } from '../../../shared/dateFormat'
import type { ColumnDef } from '@tanstack/react-table'

import type { Translate } from '../../../app/i18n/i18n'
import {
  DATA_TABLE_ROW_ACTION_KINDS,
  DataTableRowActions,
} from '../../../components/ui/data-table-row-actions'
import type { SystemLogLiteDto } from './systemLogTypes'
import { getSystemLogLevelEnum, getSystemLogStatusEnum } from './systemLogUi'

type CreateSystemLogTableColumnsRequest = {
  onView: (systemLog: SystemLogLiteDto) => void
  locale: string
  t: Translate
}

export function createSystemLogTableColumns({
  locale,
  onView,
  t,
}: CreateSystemLogTableColumnsRequest): ColumnDef<SystemLogLiteDto>[] {
  return [
    {
      accessorKey: 'createdAt',
      cell: ({ row }) => formatUserDateTime(row.original.createdAt, locale),
      header: t('common.labels.createdAt'),
    },
    {
      accessorKey: 'level',
      cell: ({ row }) => getSystemLogLevelEnum(row.original.level, t),
      header: t('features.sentinel.labels.level'),
    },
    {
      accessorKey: 'status',
      cell: ({ row }) => getSystemLogStatusEnum(row.original.status, t),
      header: t('common.labels.status'),
    },
    {
      accessorKey: 'module',
      header: t('common.labels.module'),
    },
    {
      accessorKey: 'message',
      header: t('common.labels.message'),
    },
    {
      cell: ({ row }) => (
        <DataTableRowActions
          actions={[
            {
              kind: DATA_TABLE_ROW_ACTION_KINDS.view,
              label: t('common.actions.view'),
              onClick: () => onView(row.original),
            },
          ]}
        />
      ),
      enableHiding: false,
      enableSorting: false,
      header: t('common.labels.actions'),
      id: 'actions',
    },
  ]
}
