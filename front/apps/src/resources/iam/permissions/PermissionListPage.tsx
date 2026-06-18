import { useForm } from '@tanstack/react-form'
import type { SortingState } from '@tanstack/react-table'
import { useCallback, useEffect, useMemo, useState } from 'react'

import { useTranslate } from '../../../app/i18n/i18n'
import { useAuth, useNotifyError } from '../../../auth/AuthProvider'
import { DataTable } from '../../../components/ui/data-table'
import { Field, FieldLabel } from '../../../components/ui/form'
import { FilterToolbar } from '../../../components/ui/filter-toolbar'
import { Input } from '../../../components/ui/input'
import { Select } from '../../../components/ui/select'
import { IAM_PERMISSIONS } from '../../../shared/iamConstants'
import type { PermissionDto } from '../../../shared/permissions'
import { hasPermissionCode } from '../../../shared/permissions'
import { getPermissions } from './permissionApi'
import { PermissionEditDialog } from './PermissionEditDialog'
import { createPermissionTableColumns } from './PermissionListPageColumns'
import {
  PERMISSION_FILTER_VALUES,
  PERMISSION_MODULE_OPTIONS,
  PERMISSION_RESOURCE_OPTIONS,
  type PermissionSearchForm,
} from './permissionTypes'
import { toTranslatedOptions } from './permissionUi'

const EMPTY_PERMISSION_SEARCH: PermissionSearchForm = {
  action: '',
  isActive: PERMISSION_FILTER_VALUES.all,
  module: PERMISSION_FILTER_VALUES.all,
  resource: PERMISSION_FILTER_VALUES.all,
  title: '',
}

export function PermissionListPage() {
  const t = useTranslate()
  const notifyError = useNotifyError()
  const { permissions: userPermissions } = useAuth()
  const [permissions, setPermissions] = useState<PermissionDto[]>([])
  const [selectedPermission, setSelectedPermission] = useState<PermissionDto | null>(null)
  const [appliedFilters, setAppliedFilters] = useState<PermissionSearchForm>(EMPTY_PERMISSION_SEARCH)
  const [isLoading, setIsLoading] = useState(false)
  const [sorting, setSorting] = useState<SortingState>([])
  const canUpdate = hasPermissionCode(userPermissions, IAM_PERMISSIONS.permissions.write)
  const filterForm = useForm({
    defaultValues: EMPTY_PERMISSION_SEARCH,
    onSubmit: ({ value }) => {
      setAppliedFilters({ ...value })
    },
  })
  const columns = useMemo(() => createPermissionTableColumns({
    canUpdate,
    onEdit: setSelectedPermission,
    t,
  }), [canUpdate, t])

  const loadPermissions = useCallback(async () => {
    setIsLoading(true)

    try {
      setPermissions(await getPermissions(appliedFilters))
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    } finally {
      setIsLoading(false)
    }
  }, [appliedFilters, notifyError, t])

  useEffect(() => {
    void loadPermissions()
  }, [loadPermissions])

  function handleReset() {
    filterForm.reset()
    setAppliedFilters(EMPTY_PERMISSION_SEARCH)
  }

  async function handleSaved() {
    await loadPermissions()
  }

  return (
    <main className="page">
      <div className="page-header">
        <h1 className="page-title">{t('features.iam.permissions.pages.list')}</h1>
      </div>
      <FilterToolbar onReset={handleReset} onSubmit={(event) => {
        event.preventDefault()
        void filterForm.handleSubmit()
      }}>
        <filterForm.Field name="module">
          {(field) => (
            <Field>
              <FieldLabel>{t('shared.fields.module')}</FieldLabel>
              <Select
                onValueChange={field.handleChange}
                options={[{ label: t('shared.filters.all'), value: PERMISSION_FILTER_VALUES.all }, ...toTranslatedOptions(PERMISSION_MODULE_OPTIONS, t)]}
                value={field.state.value}
              />
            </Field>
          )}
        </filterForm.Field>
        <filterForm.Field name="resource">
          {(field) => (
            <Field>
              <FieldLabel>{t('shared.fields.resource')}</FieldLabel>
              <Select
                onValueChange={field.handleChange}
                options={[{ label: t('shared.filters.all'), value: PERMISSION_FILTER_VALUES.all }, ...toTranslatedOptions(PERMISSION_RESOURCE_OPTIONS, t)]}
                value={field.state.value}
              />
            </Field>
          )}
        </filterForm.Field>
        <filterForm.Field name="action">
          {(field) => (
            <Field>
              <FieldLabel htmlFor={field.name}>{t('shared.fields.action')}</FieldLabel>
              <Input id={field.name} onBlur={field.handleBlur} onChange={(event) => field.handleChange(event.currentTarget.value)} value={field.state.value} />
            </Field>
          )}
        </filterForm.Field>
        <filterForm.Field name="title">
          {(field) => (
            <Field>
              <FieldLabel htmlFor={field.name}>{t('shared.fields.title')}</FieldLabel>
              <Input id={field.name} onBlur={field.handleBlur} onChange={(event) => field.handleChange(event.currentTarget.value)} value={field.state.value} />
            </Field>
          )}
        </filterForm.Field>
      </FilterToolbar>
      <DataTable
        columns={columns}
        data={permissions}
        emptyText={t('features.iam.permissions.messages.empty')}
        isLoading={isLoading}
        loadingText={t('shared.common.loading')}
        onSortingChange={setSorting}
        sorting={sorting}
      />
      {selectedPermission !== null && (
        <PermissionEditDialog
          isOpen={selectedPermission !== null}
          onClose={() => setSelectedPermission(null)}
          onSaved={handleSaved}
          permission={selectedPermission}
        />
      )}
    </main>
  )
}
