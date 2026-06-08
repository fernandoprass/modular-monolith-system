import {
  ActionIcon,
  Badge,
  Button,
  Group,
  Pagination,
  Paper,
  Select,
  Table,
  Text,
  TextInput,
  Tooltip,
} from '@mantine/core'
import { modals } from '@mantine/modals'
import { notifications } from '@mantine/notifications'
import { IconEdit, IconEye, IconTrash } from '@tabler/icons-react'
import { useCallback, useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'

import { APP_ROUTES } from '../../../app/routes'
import { useTranslate } from '../../../app/i18n/i18n'
import { notifyError } from '../../../auth/AuthProvider'
import { IAM_PERMISSIONS } from '../../../shared/iamConstants'
import { hasPermissionCode } from '../../../shared/permissions'
import { useAuth } from '../../../auth/AuthProvider'
import {
  ORGANIZATION_TYPE_OPTIONS,
  type OrganizationDto,
  type PagedResultDto,
} from './organizationTypes'
import { deleteOrganization, getOrganizations } from './organizationApi'
import { getLanguageLabel, getOrganizationTypeLabel, toTranslatedOptions } from './organizationUi'

const DEFAULT_PAGE_NUMBER = 1
const DEFAULT_PAGE_SIZE = 25
const ICON_SIZE = 16

export function OrganizationListPage() {
  const t = useTranslate()
  const navigate = useNavigate()
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
  const canView = hasPermissionCode(permissions, IAM_PERMISSIONS.organizations.view)
  const canUpdate = hasPermissionCode(permissions, IAM_PERMISSIONS.organizations.update)
  const canDelete = hasPermissionCode(permissions, IAM_PERMISSIONS.organizations.delete)

  const loadOrganizations = useCallback(async (targetPage = pageNumber) => {
    setIsLoading(true)

    try {
      const organizations = await getOrganizations({
        code: appliedCodeFilter,
        name: appliedNameFilter,
        pageNumber: targetPage,
        pageSize: DEFAULT_PAGE_SIZE,
        type: appliedTypeFilter,
      })
      setResult(organizations)
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    } finally {
      setIsLoading(false)
    }
  }, [appliedCodeFilter, appliedNameFilter, appliedTypeFilter, pageNumber, t])

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

  function confirmDelete(organization: OrganizationDto) {
    modals.openConfirmModal({
      centered: true,
      children: <Text size="sm">{t('resources.iam.organizations.messages.deleteConfirm')}</Text>,
      labels: {
        cancel: t('shared.actions.cancel'),
        confirm: t('resources.iam.organizations.actions.delete'),
      },
      onConfirm: async () => {
        try {
          await deleteOrganization(organization.id)
          notifications.show({
            color: 'green',
            message: t('resources.iam.organizations.notifications.deleted'),
          })
          await loadOrganizations(pageNumber)
        } catch (error) {
          notifyError(error, t('shared.errors.generic'))
        }
      },
      title: t('resources.iam.organizations.actions.delete'),
    })
  }

  const totalPages = result?.totalPages ?? 1

  return (
    <main className="page">
      <Group justify="space-between" mb="sm">
        <div>
          <h1 className="page-title">{t('resources.iam.organizations.pages.list')}</h1>
        </div>
      </Group>

      <Paper className="toolbar" p="xs" withBorder>
        <form onSubmit={handleFilter}>
          <Group align="end" gap="xs">
            <TextInput
              label={t('resources.iam.organizations.fields.code')}
              onChange={(event) => setCodeFilter(event.currentTarget.value)}
              value={codeFilter}
            />
            <TextInput
              label={t('resources.iam.organizations.fields.name')}
              onChange={(event) => setNameFilter(event.currentTarget.value)}
              value={nameFilter}
            />
            <Select
              clearable
              data={toTranslatedOptions(ORGANIZATION_TYPE_OPTIONS, t)}
              label={t('resources.iam.organizations.fields.type')}
              onChange={setTypeFilter}
              value={typeFilter}
            />
            <Button type="submit">{t('resources.iam.organizations.actions.filter')}</Button>
            <Button variant="default" onClick={handleReset}>{t('resources.iam.organizations.actions.reset')}</Button>
          </Group>
        </form>
      </Paper>

      <Paper className="table-panel" withBorder>
        <Table striped highlightOnHover withTableBorder={false} verticalSpacing={4}>
          <Table.Thead>
            <Table.Tr>
              <Table.Th>{t('resources.iam.organizations.fields.type')}</Table.Th>
              <Table.Th>{t('resources.iam.organizations.fields.code')}</Table.Th>
              <Table.Th>{t('resources.iam.organizations.fields.name')}</Table.Th>
              <Table.Th>{t('resources.iam.organizations.fields.defaultLanguage')}</Table.Th>
              <Table.Th>{t('resources.iam.organizations.fields.isActive')}</Table.Th>
              <Table.Th className="actions-column">{t('resources.iam.organizations.fields.actions')}</Table.Th>
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {result?.items.map((organization) => (
              <Table.Tr key={organization.id}>
                <Table.Td>{getOrganizationTypeLabel(organization.type, t)}</Table.Td>
                <Table.Td>{organization.code}</Table.Td>
                <Table.Td>{organization.name}</Table.Td>
                <Table.Td>{getLanguageLabel(organization.defaultLanguage, t)}</Table.Td>
                <Table.Td>
                  <Badge color={organization.isActive ? 'green' : 'gray'} variant="light">
                    {organization.isActive ? t('shared.status.active') : t('shared.status.inactive')}
                  </Badge>
                </Table.Td>
                <Table.Td>
                  <Group gap={4} justify="flex-end">
                    {canView && (
                      <Tooltip label={t('resources.iam.organizations.actions.view')}>
                        <ActionIcon onClick={() => navigate(APP_ROUTES.organizationShow(organization.id))}>
                          <IconEye size={ICON_SIZE} />
                        </ActionIcon>
                      </Tooltip>
                    )}
                    {canUpdate && (
                      <Tooltip label={t('resources.iam.organizations.actions.edit')}>
                        <ActionIcon onClick={() => navigate(APP_ROUTES.organizationEdit(organization.id))}>
                          <IconEdit size={ICON_SIZE} />
                        </ActionIcon>
                      </Tooltip>
                    )}
                    {canDelete && (
                      <Tooltip label={t('resources.iam.organizations.actions.delete')}>
                        <ActionIcon color="red" onClick={() => confirmDelete(organization)}>
                          <IconTrash size={ICON_SIZE} />
                        </ActionIcon>
                      </Tooltip>
                    )}
                  </Group>
                </Table.Td>
              </Table.Tr>
            ))}
          </Table.Tbody>
        </Table>
        {result !== null && result.items.length === 0 && (
          <Text c="dimmed" p="md" ta="center" size="sm">
            {t('resources.iam.organizations.messages.empty')}
          </Text>
        )}
        {isLoading && (
          <Text c="dimmed" p="md" ta="center" size="sm">
            {t('shared.common.loading')}
          </Text>
        )}
      </Paper>

      <Group justify="space-between" mt="sm">
        <Text size="xs" c="dimmed">
          {t('shared.pagination.summary', {
            page: result?.pageNumber ?? pageNumber,
            pages: totalPages,
            total: result?.totalCount ?? 0,
          })}
        </Text>
        <Pagination
          total={totalPages}
          value={pageNumber}
          onChange={setPageNumber}
          size="sm"
        />
      </Group>
    </main>
  )
}
