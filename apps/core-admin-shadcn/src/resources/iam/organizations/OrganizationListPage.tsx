import { Edit, Eye, Trash2 } from 'lucide-react'
import { useCallback, useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'

import { useToast } from '../../../app/ToastProvider'
import { useTranslate } from '../../../app/i18n/i18n'
import { APP_ROUTES } from '../../../app/routes'
import { useAuth, useNotifyError } from '../../../auth/AuthProvider'
import { Badge } from '../../../components/ui/badge'
import { Button } from '../../../components/ui/button'
import { ConfirmDialog } from '../../../components/ui/dialog'
import { Input } from '../../../components/ui/input'
import { Label } from '../../../components/ui/label'
import { Select } from '../../../components/ui/select'
import { IAM_PERMISSIONS } from '../../../shared/iamConstants'
import { hasPermissionCode } from '../../../shared/permissions'
import { deleteOrganization, getOrganizations } from './organizationApi'
import { ORGANIZATION_TYPE_OPTIONS, type OrganizationDto, type PagedResultDto } from './organizationTypes'
import { getLanguageLabel, getOrganizationTypeLabel, toTranslatedOptions } from './organizationUi'

const DEFAULT_PAGE_NUMBER = 1
const DEFAULT_PAGE_SIZE = 25
const ICON_SIZE = 15

export function OrganizationListPage() {
  const t = useTranslate()
  const navigate = useNavigate()
  const notifyError = useNotifyError()
  const { showSuccess } = useToast()
  const { permissions } = useAuth()
  const [codeFilter, setCodeFilter] = useState('')
  const [nameFilter, setNameFilter] = useState('')
  const [typeFilter, setTypeFilter] = useState<string | null>(null)
  const [appliedCodeFilter, setAppliedCodeFilter] = useState('')
  const [appliedNameFilter, setAppliedNameFilter] = useState('')
  const [appliedTypeFilter, setAppliedTypeFilter] = useState<string | null>(null)
  const [pageNumber, setPageNumber] = useState(DEFAULT_PAGE_NUMBER)
  const [result, setResult] = useState<PagedResultDto<OrganizationDto> | null>(null)
  const [isLoading, setIsLoading] = useState(false)
  const [deleteTarget, setDeleteTarget] = useState<OrganizationDto | null>(null)
  const canView = hasPermissionCode(permissions, IAM_PERMISSIONS.organizations.view)
  const canUpdate = hasPermissionCode(permissions, IAM_PERMISSIONS.organizations.update)
  const canDelete = hasPermissionCode(permissions, IAM_PERMISSIONS.organizations.delete)

  const loadOrganizations = useCallback(async (targetPage = pageNumber) => {
    setIsLoading(true)

    try {
      setResult(await getOrganizations({
        code: appliedCodeFilter,
        name: appliedNameFilter,
        pageNumber: targetPage,
        pageSize: DEFAULT_PAGE_SIZE,
        type: appliedTypeFilter,
      }))
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    } finally {
      setIsLoading(false)
    }
  }, [appliedCodeFilter, appliedNameFilter, appliedTypeFilter, notifyError, pageNumber, t])

  useEffect(() => {
    void loadOrganizations(pageNumber)
  }, [loadOrganizations, pageNumber])

  function handleFilter(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setPageNumber(DEFAULT_PAGE_NUMBER)
    setAppliedCodeFilter(codeFilter)
    setAppliedNameFilter(nameFilter)
    setAppliedTypeFilter(typeFilter)
  }

  function handleReset() {
    setCodeFilter('')
    setNameFilter('')
    setTypeFilter(null)
    setAppliedCodeFilter('')
    setAppliedNameFilter('')
    setAppliedTypeFilter(null)
    setPageNumber(DEFAULT_PAGE_NUMBER)
  }

  async function handleConfirmDelete() {
    if (deleteTarget === null) {
      return
    }

    try {
      await deleteOrganization(deleteTarget.id)
      showSuccess(t('resources.iam.organizations.notifications.deleted'))
      setDeleteTarget(null)
      await loadOrganizations(pageNumber)
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    }
  }

  const totalPages = result?.totalPages ?? 1

  return (
    <main className="page">
      <h1 className="page-title">{t('resources.iam.organizations.pages.list')}</h1>
      <form className="toolbar" onSubmit={handleFilter}>
        <Field label={t('resources.iam.organizations.fields.code')}>
          <Input onChange={(event) => setCodeFilter(event.currentTarget.value)} value={codeFilter} />
        </Field>
        <Field label={t('resources.iam.organizations.fields.name')}>
          <Input onChange={(event) => setNameFilter(event.currentTarget.value)} value={nameFilter} />
        </Field>
        <Field label={t('resources.iam.organizations.fields.type')}>
          <Select
            onValueChange={(value) => setTypeFilter(value === 'all' ? null : value)}
            options={[{ label: '', value: 'all' }, ...toTranslatedOptions(ORGANIZATION_TYPE_OPTIONS, t)]}
            value={typeFilter ?? 'all'}
          />
        </Field>
        <Button type="submit">{t('resources.iam.organizations.actions.filter')}</Button>
        <Button onClick={handleReset} type="button" variant="outline">{t('resources.iam.organizations.actions.reset')}</Button>
      </form>

      <div className="table-panel">
        <table className="data-table">
          <thead>
            <tr>
              <th>{t('resources.iam.organizations.fields.type')}</th>
              <th>{t('resources.iam.organizations.fields.code')}</th>
              <th>{t('resources.iam.organizations.fields.name')}</th>
              <th>{t('resources.iam.organizations.fields.defaultLanguage')}</th>
              <th>{t('resources.iam.organizations.fields.isActive')}</th>
              <th className="actions-column">{t('resources.iam.organizations.fields.actions')}</th>
            </tr>
          </thead>
          <tbody>
            {result?.items.map((organization) => (
              <tr key={organization.id}>
                <td>{getOrganizationTypeLabel(organization.type, t)}</td>
                <td>{organization.code}</td>
                <td>{organization.name}</td>
                <td>{getLanguageLabel(organization.defaultLanguage, t)}</td>
                <td>
                  <Badge variant={organization.isActive ? 'active' : 'inactive'}>
                    {organization.isActive ? t('shared.status.active') : t('shared.status.inactive')}
                  </Badge>
                </td>
                <td>
                  <div className="row-actions">
                    {canView && (
                      <Button onClick={() => navigate(APP_ROUTES.organizationShow(organization.id))} size="icon" title={t('resources.iam.organizations.actions.view')} variant="ghost">
                        <Eye size={ICON_SIZE} />
                      </Button>
                    )}
                    {canUpdate && (
                      <Button onClick={() => navigate(APP_ROUTES.organizationEdit(organization.id))} size="icon" title={t('resources.iam.organizations.actions.edit')} variant="ghost">
                        <Edit size={ICON_SIZE} />
                      </Button>
                    )}
                    {canDelete && (
                      <Button onClick={() => setDeleteTarget(organization)} size="icon" title={t('resources.iam.organizations.actions.delete')} variant="ghost">
                        <Trash2 size={ICON_SIZE} />
                      </Button>
                    )}
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        {result !== null && result.items.length === 0 && <p className="empty-text">{t('resources.iam.organizations.messages.empty')}</p>}
        {isLoading && <p className="empty-text">{t('shared.common.loading')}</p>}
      </div>

      <div className="pagination-row">
        <span>
          {t('shared.pagination.summary', {
            page: result?.pageNumber ?? pageNumber,
            pages: totalPages,
            total: result?.totalCount ?? 0,
          })}
        </span>
        <div className="pagination-actions">
          <Button disabled={pageNumber <= 1} onClick={() => setPageNumber((page) => page - 1)} type="button" variant="outline">
            Prev
          </Button>
          <Button disabled={pageNumber >= totalPages} onClick={() => setPageNumber((page) => page + 1)} type="button" variant="outline">
            Next
          </Button>
        </div>
      </div>

      <ConfirmDialog
        cancelText={t('shared.actions.cancel')}
        confirmText={t('resources.iam.organizations.actions.delete')}
        onConfirm={() => void handleConfirmDelete()}
        onOpenChange={(open) => !open && setDeleteTarget(null)}
        open={deleteTarget !== null}
        title={t('resources.iam.organizations.actions.delete')}
      >
        <p>{t('resources.iam.organizations.messages.deleteConfirm')}</p>
      </ConfirmDialog>
    </main>
  )
}

type FieldProps = {
  children: React.ReactNode
  label: string
}

function Field({ children, label }: FieldProps) {
  return (
    <div className="field">
      <Label>{label}</Label>
      {children}
    </div>
  )
}
