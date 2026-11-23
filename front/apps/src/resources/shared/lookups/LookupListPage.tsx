import type { SortingState } from '@tanstack/react-table'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { Navigate } from 'react-router-dom'
import { useTranslate } from '../../../app/i18n/i18n'
import { APP_ROUTES } from '../../../app/routes'
import { useAuth, useNotifyError } from '../../../auth/AuthProvider'
import { DataTable } from '../../../components/ui/data-table'
import { DataTablePagination } from '../../../components/ui/data-table-pagination'
import { FilterToolbar } from '../../../components/ui/filter-toolbar'
import { Field, FieldLabel } from '../../../components/ui/form'
import { Input } from '../../../components/ui/input'
import { Select } from '../../../components/ui/select'
import { LANGUAGE_CODES, LANGUAGE_OPTIONS, normalizeLanguageCode, type LanguageCode } from '../../../shared/languages'
import { DEFAULT_PAGINATION, type PagedResultDto } from '../../../shared/pagination'
import { hasPermissionCode } from '../../../shared/permissions'
import { SHARED_PERMISSIONS } from '../../../shared/sharedConstants'
import { toTranslatedOptions } from '../../../shared/translatedOptions'
import { getLookups } from './lookupApi'
import { createLookupTableColumns } from './LookupListPageColumns'
import { LOOKUP_FILTER_VALUES, LOOKUP_TYPE_ENUM, type LookupLiteDto, type LookupSearchForm } from './lookupTypes'
import { getLookupTypeEnum } from './lookupUi'

const EMPTY_LOOKUP_SEARCH: LookupSearchForm = {
  code: '', languageCode: LANGUAGE_CODES.english, type: LOOKUP_FILTER_VALUES.all, value: '',
}

export function LookupListPage() {
  const { permissions, user } = useAuth()
  const isSystemAdmin = user?.isSystemAdmin === true
  const canRead = isSystemAdmin || hasPermissionCode(permissions, SHARED_PERMISSIONS.lookups.read)
  const language = normalizeLanguageCode(user?.language ?? LANGUAGE_CODES.english)
  const defaultLanguage = LANGUAGE_OPTIONS.find(option => option.value === language)?.value ?? LANGUAGE_CODES.english

  if (!canRead) return <Navigate to={APP_ROUTES.dashboard} replace />

  return <LookupSearchPage key={`${user?.id}-${defaultLanguage}-${isSystemAdmin}`} defaultLanguage={defaultLanguage} isSystemAdmin={isSystemAdmin} />
}

function LookupSearchPage({ defaultLanguage, isSystemAdmin }: { defaultLanguage: LanguageCode; isSystemAdmin: boolean }) {
  const t = useTranslate()
  const notifyError = useNotifyError()
  const emptySearch = useMemo(() => ({ ...EMPTY_LOOKUP_SEARCH, languageCode: defaultLanguage }), [defaultLanguage])
  const [appliedFilters, setAppliedFilters] = useState<LookupSearchForm>(emptySearch)
  const [pageNumber, setPageNumber] = useState<number>(DEFAULT_PAGINATION.pageNumber)
  const [pageSize, setPageSize] = useState<number>(DEFAULT_PAGINATION.pageSize)
  const [result, setResult] = useState<PagedResultDto<LookupLiteDto> | null>(null)
  const [isLoading, setIsLoading] = useState(false)
  const [sorting, setSorting] = useState<SortingState>([])
  const { control, handleSubmit, register, reset } = useForm<LookupSearchForm>({ defaultValues: emptySearch })
  const columns = useMemo(() => createLookupTableColumns(t), [t])

  const loadLookups = useCallback(async (isCurrent: () => boolean) => {
    setIsLoading(true)
    try {
      const loaded = await getLookups({ ...appliedFilters, type: isSystemAdmin ? appliedFilters.type : LOOKUP_FILTER_VALUES.all, pageNumber, pageSize })
      if (isCurrent()) setResult(loaded)
    } catch (error) {
      if (isCurrent()) {
        setResult(null)
        notifyError(error, t('common.messages.errors.generic'))
      }
    } finally {
      if (isCurrent()) setIsLoading(false)
    }
  }, [appliedFilters, isSystemAdmin, notifyError, pageNumber, pageSize, t])

  useEffect(() => {
    let cancelled = false
    void loadLookups(() => !cancelled)
    return () => { cancelled = true }
  }, [loadLookups])

  function handleSearch(value: LookupSearchForm) {
    setPageNumber(DEFAULT_PAGINATION.pageNumber)
    setAppliedFilters({ ...value })
  }

  function handleReset() {
    reset(emptySearch)
    setAppliedFilters({ ...emptySearch })
    setPageNumber(DEFAULT_PAGINATION.pageNumber)
  }

  function handlePageSizeChange(value: number) {
    setPageSize(value)
    setPageNumber(DEFAULT_PAGINATION.pageNumber)
  }

  return (
    <main className="page">
      <div className="page-header"><h1 className="page-title">{t('features.shared.labels.lookups')}</h1></div>
      <FilterToolbar onReset={handleReset} onSubmit={handleSubmit(handleSearch)}>
        {isSystemAdmin && <Controller control={control} name="type" render={({ field }) => (
          <Field>
            <FieldLabel htmlFor="lookup-type">{t('common.labels.type')}</FieldLabel>
            <Select id="lookup-type" onValueChange={field.onChange} value={field.value} options={[
              { label: t('common.actions.all'), value: LOOKUP_FILTER_VALUES.all },
              ...Object.values(LOOKUP_TYPE_ENUM).map(value => ({ label: getLookupTypeEnum(value, t), value })),
            ]} />
          </Field>
        )} />}
        <Controller control={control} name="languageCode" render={({ field }) => (
          <Field>
            <FieldLabel htmlFor="lookup-language">{t('common.labels.language')}</FieldLabel>
            <Select id="lookup-language" onValueChange={field.onChange} value={field.value} options={toTranslatedOptions(LANGUAGE_OPTIONS, t)} />
          </Field>
        )} />
        <Field>
          <FieldLabel htmlFor="lookup-code">{t('common.labels.code')}</FieldLabel>
          <Input id="lookup-code" {...register('code')} />
        </Field>
        <Field>
          <FieldLabel htmlFor="lookup-value">{t('common.labels.value')}</FieldLabel>
          <Input id="lookup-value" {...register('value')} />
        </Field>
      </FilterToolbar>
      <DataTable columns={columns} data={result?.items ?? []} emptyText={t('common.states.noResults', { entities: t('app.entities.lookups') })}
        isLoading={isLoading} loadingText={t('common.states.loading')} onSortingChange={setSorting} sorting={sorting} />
      <DataTablePagination onPageChange={setPageNumber} onPageSizeChange={handlePageSizeChange}
        pageNumber={result?.pageNumber ?? pageNumber} pageSize={pageSize} totalCount={result?.totalCount ?? 0} totalPages={result?.totalPages ?? 0} />
    </main>
  )
}
