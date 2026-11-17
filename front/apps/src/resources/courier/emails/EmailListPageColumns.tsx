import type { ColumnDef } from '@tanstack/react-table'

import type { Translate } from '../../../app/i18n/i18n'
import { Badge } from '../../../components/ui/badge'
import {
  DATA_TABLE_ROW_ACTION_KINDS,
  DataTableRowActions,
} from '../../../components/ui/data-table-row-actions'
import { formatUserDateTime } from '../../../shared/dateFormat'
import type { EmailLiteDto } from './emailTypes'
import { getEmailStatusClassName, getEmailStatusEnum } from './emailUi'

type CreateEmailTableColumnsRequest = {
  language?: string
  onView: (email: EmailLiteDto) => void
  t: Translate
}

export function createEmailTableColumns({
  language,
  onView,
  t,
}: CreateEmailTableColumnsRequest): ColumnDef<EmailLiteDto>[] {
  return [
    {
      accessorKey: 'createdAt',
      cell: ({ row }) => formatUserDateTime(row.original.createdAt, language),
      header: t('common.labels.createdAt'),
    },
    {
      accessorKey: 'module',
      header: t('common.labels.module'),
    },
    {
      accessorKey: 'feature',
      header: t('common.labels.feature'),
    },
    {
      accessorKey: 'recipient',
      header: t('features.courier.labels.recipient'),
    },
    {
      accessorKey: 'subject',
      header: t('features.courier.labels.subject'),
    },
    {
      accessorKey: 'status',
      cell: ({ row }) => (
        <Badge className={getEmailStatusClassName(row.original.status)}>
          {getEmailStatusEnum(row.original.status, t)}
        </Badge>
      ),
      header: t('common.labels.status'),
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
