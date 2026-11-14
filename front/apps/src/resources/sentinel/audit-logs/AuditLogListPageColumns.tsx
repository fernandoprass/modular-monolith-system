import { formatUserDateTime } from '../../../shared/dateFormat'
import type { ColumnDef } from '@tanstack/react-table'

import type { Translate } from '../../../app/i18n/i18n'
import {
  DATA_TABLE_ROW_ACTION_KINDS,
  DataTableRowActions,
} from '../../../components/ui/data-table-row-actions'
import type { AuditLogLiteDto } from './auditLogTypes'

type CreateAuditLogTableColumnsRequest = {
  onView: (auditLog: AuditLogLiteDto) => void
  locale: string
  t: Translate
}

export function createAuditLogTableColumns({
  locale,
  onView,
  t,
}: CreateAuditLogTableColumnsRequest): ColumnDef<AuditLogLiteDto>[] {
  return [
    {
      accessorKey: 'createdAt',
      cell: ({ row }) => formatUserDateTime(row.original.createdAt, locale),
      header: t('common.fields.createdAt'),
    },
    {
      accessorKey: 'module',
      header: t('common.fields.module'),
    },
    {
      accessorKey: 'feature',
      header: t('common.fields.feature'),
    },
    {
      accessorKey: 'action',
      header: t('common.fields.action'),
    },
    {
      accessorKey: 'description',
      header: t('common.fields.description'),
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
      header: t('common.fields.actions'),
      id: 'actions',
    },
  ]
}
