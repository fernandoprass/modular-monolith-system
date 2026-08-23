import type { ColumnDef } from '@tanstack/react-table'
import { Check, ExternalLink } from 'lucide-react'

import type { Translate } from '../../../app/i18n/i18n'
import { Badge } from '../../../components/ui/badge'
import {
  DATA_TABLE_ROW_ACTION_KINDS,
  DataTableRowActions,
} from '../../../components/ui/data-table-row-actions'
import { formatUserDateTime } from '../../../shared/dateFormat'
import { NOTIFICATION_STATUSES, type NotificationLiteDto } from './notificationTypes'
import { getNotificationStatusClassName, getNotificationStatusLabel } from './notificationUi'

type CreateNotificationTableColumnsRequest = {
  canDelete: boolean
  canMarkRead: boolean
  language?: string
  onDelete: (notification: NotificationLiteDto) => void
  onMarkRead: (notification: NotificationLiteDto) => void
  onOpenLink: (notification: NotificationLiteDto) => void
  t: Translate
}

export function createNotificationTableColumns({
  canDelete,
  canMarkRead,
  language,
  onDelete,
  onMarkRead,
  onOpenLink,
  t,
}: CreateNotificationTableColumnsRequest): ColumnDef<NotificationLiteDto>[] {
  return [
    {
      accessorKey: 'createdAt',
      cell: ({ row }) => formatUserDateTime(row.original.createdAt, language),
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
      accessorKey: 'title',
      header: t('shared.fields.title'),
    },
    {
      accessorKey: 'message',
      header: t('shared.fields.message'),
    },
    {
      accessorKey: 'status',
      cell: ({ row }) => (
        <Badge className={getNotificationStatusClassName(row.original.status)}>
          {getNotificationStatusLabel(row.original.status, t)}
        </Badge>
      ),
      header: t('shared.fields.status'),
    },
    {
      accessorKey: 'readAt',
      cell: ({ row }) => row.original.readAt === null ? '' : formatUserDateTime(row.original.readAt, language),
      header: t('shared.fields.readAt'),
    },
    {
      cell: ({ row }) => (
        <DataTableRowActions
          actions={[
            {
              icon: <ExternalLink data-icon="inline-start" />,
              isVisible: row.original.actionLink.length > 0,
              key: 'open',
              label: t('features.courier.notifications.actions.openLink'),
              onClick: () => onOpenLink(row.original),
            },
            {
              icon: <Check data-icon="inline-start" />,
              isVisible: canMarkRead && row.original.status === NOTIFICATION_STATUSES.unread,
              key: 'read',
              label: t('features.courier.notifications.actions.markRead'),
              onClick: () => onMarkRead(row.original),
            },
            {
              isVisible: canDelete,
              kind: DATA_TABLE_ROW_ACTION_KINDS.delete,
              label: t('shared.actions.delete'),
              onClick: () => onDelete(row.original),
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
