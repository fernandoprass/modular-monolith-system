import type { ColumnDef } from '@tanstack/react-table'

import type { Translate } from '../../../app/i18n/i18n'
import { Badge } from '../../../components/ui/badge'
import {
  DATA_TABLE_ROW_ACTION_KINDS,
  DataTableRowActions,
} from '../../../components/ui/data-table-row-actions'
import type { EmailLiteDto } from './emailTypes'
import { getEmailStatusClassName, getEmailStatusLabel } from './emailUi'

type CreateEmailTableColumnsRequest = {
  onView: (email: EmailLiteDto) => void
  t: Translate
}

export function createEmailTableColumns({
  onView,
  t,
}: CreateEmailTableColumnsRequest): ColumnDef<EmailLiteDto>[] {
  return [
    {
      accessorKey: 'module',
      header: t('shared.fields.module'),
    },
    {
      accessorKey: 'feature',
      header: t('shared.fields.feature'),
    },
    {
      accessorKey: 'recipient',
      header: t('shared.fields.recipient'),
    },
    {
      accessorKey: 'subject',
      header: t('shared.fields.subject'),
    },
    {
      accessorKey: 'status',
      cell: ({ row }) => (
        <Badge className={getEmailStatusClassName(row.original.status)}>
          {getEmailStatusLabel(row.original.status, t)}
        </Badge>
      ),
      header: t('shared.fields.status'),
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
