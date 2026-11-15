import { getLanguageLabel } from '../../../shared/translatedOptions'
import type { ColumnDef } from '@tanstack/react-table'
import type { NavigateFunction } from 'react-router-dom'

import type { Translate } from '../../../app/i18n/i18n'
import { APP_ROUTES } from '../../../app/routes'
import { Badge } from '../../../components/ui/badge'
import { DATA_TABLE_ROW_ACTION_KINDS, DataTableRowActions } from '../../../components/ui/data-table-row-actions'
import type { OrganizationDto } from './organizationTypes'
import { getOrganizationTypeEnum } from './organizationUi'

type OrganizationTableColumnsOptions = {
  canDelete: boolean
  canUpdate: boolean
  canView: boolean
  navigate: NavigateFunction
  setDeleteTarget: (organization: OrganizationDto) => void
  t: Translate
}

export function createOrganizationTableColumns({
  canDelete,
  canUpdate,
  canView,
  navigate,
  setDeleteTarget,
  t,
}: OrganizationTableColumnsOptions): ColumnDef<OrganizationDto>[] {
  return [
    {
      accessorKey: 'type',
      cell: ({ row }) => getOrganizationTypeEnum(row.original.type, t),
      header: t('common.fields.type'),
      sortingFn: (left, right) =>
        getOrganizationTypeEnum(left.original.type, t).localeCompare(getOrganizationTypeEnum(right.original.type, t)),
    },
    {
      accessorKey: 'code',
      header: t('common.fields.code'),
    },
    {
      accessorKey: 'name',
      header: t('common.fields.name'),
    },
    {
      accessorKey: 'defaultLanguage',
      cell: ({ row }) => getLanguageLabel(row.original.defaultLanguage, t),
      header: t('common.fields.defaultLanguage'),
      sortingFn: (left, right) =>
        getLanguageLabel(left.original.defaultLanguage, t).localeCompare(getLanguageLabel(right.original.defaultLanguage, t)),
    },
    {
      accessorKey: 'isActive',
      cell: ({ row }) => (
        <Badge variant={row.original.isActive ? 'active' : 'inactive'}>
          {row.original.isActive ? t('common.status.active') : t('common.status.inactive')}
        </Badge>
      ),
      header: t('common.fields.isActive'),
    },
    {
      cell: ({ row }) => (
        <DataTableRowActions
          actions={[
            {
              isVisible: canView,
              kind: DATA_TABLE_ROW_ACTION_KINDS.view,
              label: t('common.actions.view'),
              onClick: () => navigate(APP_ROUTES.organizationView(row.original.id)),
            },
            {
              isVisible: canUpdate,
              kind: DATA_TABLE_ROW_ACTION_KINDS.edit,
              label: t('common.actions.edit'),
              onClick: () => navigate(APP_ROUTES.organizationEdit(row.original.id)),
            },
            {
              isVisible: canDelete,
              kind: DATA_TABLE_ROW_ACTION_KINDS.delete,
              label: t('common.actions.delete'),
              onClick: () => setDeleteTarget(row.original),
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
