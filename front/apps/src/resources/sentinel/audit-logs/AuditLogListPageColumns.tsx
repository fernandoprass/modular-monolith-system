import type { ColumnDef } from '@tanstack/react-table'

import type { Translate } from '../../../app/i18n/i18n'
import {
  DATA_TABLE_ROW_ACTION_KINDS,
  DataTableRowActions,
} from '../../../components/ui/data-table-row-actions'
import type { AuditLogLiteDto } from './auditLogTypes'

type CreateAuditLogTableColumnsRequest = {
  onView: (auditLog: AuditLogLiteDto) => void
  t: Translate
}

export function createAuditLogTableColumns({
  onView,
  t,
}: CreateAuditLogTableColumnsRequest): ColumnDef<AuditLogLiteDto>[] {
  return [
    {
      accessorKey: 'createdAt',
      header: t('shared.fields.createdAt'),
    },
    {
      accessorKey: 'module',
      header: t('shared.fields.module'),
    },
    {
      accessorKey: 'feature',
      header: t('shared.fields.feature'),
    },
    {
      accessorKey: 'action',
      header: t('shared.fields.action'),
    },
    {
      accessorKey: 'description',
      header: t('shared.fields.description'),
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
