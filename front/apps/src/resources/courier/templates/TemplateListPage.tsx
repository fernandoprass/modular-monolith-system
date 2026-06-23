import type { SortingState } from '@tanstack/react-table'
import { Plus } from 'lucide-react'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { useNavigate } from 'react-router-dom'

import { useToast } from '../../../app/ToastProvider'
import { useTranslate } from '../../../app/i18n/i18n'
import { APP_ROUTES } from '../../../app/routes'
import { useAuth, useNotifyError } from '../../../auth/AuthProvider'
import { Button } from '../../../components/ui/button'
import { DataTable } from '../../../components/ui/data-table'
import { DataTablePagination } from '../../../components/ui/data-table-pagination'
import { ConfirmDialog } from '../../../components/ui/dialog-confirm'
import { Field, FieldLabel } from '../../../components/ui/form'
import { FilterToolbar } from '../../../components/ui/filter-toolbar'
import { Input } from '../../../components/ui/input'
import { Select } from '../../../components/ui/select'
import { COURIER_PERMISSIONS } from '../../../shared/courierConstants'
import { DEFAULT_PAGINATION, type PagedResultDto } from '../../../shared/pagination'
import { hasPermissionCode } from '../../../shared/permissions'
import { deleteTemplate, getTemplates } from './templateApi'
import { createTemplateTableColumns } from './TemplateListPageColumns'
import {
  TEMPLATE_FILTER_VALUES,
  TEMPLATE_TYPE_OPTIONS,
  type TemplateLiteDto,
  type TemplateSearchForm,
} from './templateTypes'
import { toTranslatedTemplateOptions } from './templateUi'

const EMPTY_TEMPLATE_SEARCH: TemplateSearchForm = {
  key: '',
  name: '',
  type: TEMPLATE_FILTER_VALUES.all,
}

export function TemplateListPage() {
  const t = useTranslate()
  const navigate = useNavigate()
  const notifyError = useNotifyError()
  const { showSuccess } = useToast()
  const { permissions } = useAuth()
  const [appliedFilters, setAppliedFilters] = useState<TemplateSearchForm>(EMPTY_TEMPLATE_SEARCH)
  const [pageNumber, setPageNumber] = useState<number>(DEFAULT_PAGINATION.pageNumber)
  const [pageSize, setPageSize] = useState<number>(DEFAULT_PAGINATION.pageSize)
  const [result, setResult] = useState<PagedResultDto<TemplateLiteDto> | null>(null)
  const [isLoading, setIsLoading] = useState(false)
  const [sorting, setSorting] = useState<SortingState>([])
  const [templateToDelete, setTemplateToDelete] = useState<TemplateLiteDto | null>(null)
  const canWrite = hasPermissionCode(permissions, COURIER_PERMISSIONS.templates.write)
  const { control, handleSubmit, register, reset } = useForm<TemplateSearchForm>({
    defaultValues: EMPTY_TEMPLATE_SEARCH,
  })
  const columns = useMemo(() => createTemplateTableColumns({
    canDelete: canWrite,
    canUpdate: canWrite,
    onDelete: setTemplateToDelete,
    onEdit: (template) => navigate(APP_ROUTES.templateEdit(template.id)),
    t,
  }), [canWrite, navigate, t])

  const loadTemplates = useCallback(async (targetPage: number) => {
    setIsLoading(true)

    try {
      setResult(await getTemplates({
        ...appliedFilters,
        pageNumber: targetPage,
        pageSize,
      }))
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    } finally {
      setIsLoading(false)
    }
  }, [appliedFilters, notifyError, pageSize, t])

  useEffect(() => {
    void loadTemplates(pageNumber)
  }, [loadTemplates, pageNumber])

  function handleReset() {
    reset(EMPTY_TEMPLATE_SEARCH)
    setAppliedFilters(EMPTY_TEMPLATE_SEARCH)
    setPageNumber(DEFAULT_PAGINATION.pageNumber)
  }

  function handleSearch(value: TemplateSearchForm) {
    setPageNumber(DEFAULT_PAGINATION.pageNumber)
    setAppliedFilters({ ...value })
  }

  function handlePageSizeChange(nextPageSize: number) {
    setPageSize(nextPageSize)
    setPageNumber(DEFAULT_PAGINATION.pageNumber)
  }

  async function handleDelete() {
    if (templateToDelete === null) {
      return
    }

    try {
      await deleteTemplate(templateToDelete.id)
      showSuccess(t('features.courier.templates.notifications.deleted'))
      setTemplateToDelete(null)
      await loadTemplates(pageNumber)
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    }
  }

  const totalPages = result?.totalPages ?? DEFAULT_PAGINATION.pageNumber

  return (
    <main className="page courier-template-page">
      <div className="page-header">
        <h1 className="page-title">{t('features.courier.templates.pages.list')}</h1>
        {canWrite && (
          <Button onClick={() => navigate(APP_ROUTES.templateCreate)} type="button">
            <Plus data-icon="inline-start" />
            {t('features.courier.templates.actions.create')}
          </Button>
        )}
      </div>
      <FilterToolbar onReset={handleReset} onSubmit={handleSubmit(handleSearch)}>
        <Field>
          <FieldLabel htmlFor="template-key-filter">{t('shared.fields.key')}</FieldLabel>
          <Input id="template-key-filter" {...register('key')} />
        </Field>
        <Field>
          <FieldLabel htmlFor="template-name-filter">{t('shared.fields.name')}</FieldLabel>
          <Input id="template-name-filter" {...register('name')} />
        </Field>
        <Controller
          control={control}
          name="type"
          render={({ field }) => (
            <Field>
              <FieldLabel>{t('shared.fields.type')}</FieldLabel>
              <Select
                onValueChange={field.onChange}
                options={[
                  { label: t('shared.filters.all'), value: TEMPLATE_FILTER_VALUES.all },
                  ...toTranslatedTemplateOptions(TEMPLATE_TYPE_OPTIONS, t),
                ]}
                value={field.value}
              />
            </Field>
          )}
        />
      </FilterToolbar>
      <DataTable
        columns={columns}
        data={result?.items ?? []}
        emptyText={t('features.courier.templates.messages.empty')}
        isLoading={isLoading}
        loadingText={t('shared.common.loading')}
        onSortingChange={setSorting}
        sorting={sorting}
      />
      <DataTablePagination
        onPageChange={setPageNumber}
        onPageSizeChange={handlePageSizeChange}
        pageNumber={result?.pageNumber ?? pageNumber}
        pageSize={pageSize}
        totalCount={result?.totalCount ?? 0}
        totalPages={totalPages}
      />
      <ConfirmDialog
        backLabel={t('shared.actions.back')}
        cancelText={t('shared.actions.cancel')}
        confirmText={t('shared.actions.delete')}
        onConfirm={() => void handleDelete()}
        onOpenChange={(open) => !open && setTemplateToDelete(null)}
        open={templateToDelete !== null}
        title={t('features.courier.templates.messages.deleteTitle')}
      >
        {t('features.courier.templates.messages.deleteConfirm')}
      </ConfirmDialog>
    </main>
  )
}
