import { useCallback, useEffect } from 'react'

import { useTranslate } from '../../../app/i18n/i18n'
import { useAuth, useNotifyError } from '../../../auth/AuthProvider'
import { AsyncLookupSelect } from '../../../components/ui/async-lookup-select'
import { getOrganizationLookup } from './organizationApi'
import type { OrganizationLookupDto } from './organizationTypes'

type OrganizationSelectProps = {
  clearable?: boolean
  disabled?: boolean
  includeInactive?: boolean
  onValueChange: (value: string) => void
  value: string
}

function getOptionLabel(organization: OrganizationLookupDto): string {
  return `${organization.name} (${organization.code})`
}

export function OrganizationSelect({
  clearable = false,
  disabled = false,
  includeInactive = false,
  onValueChange,
  value,
}: OrganizationSelectProps) {
  const t = useTranslate()
  const { user } = useAuth()
  const notifyError = useNotifyError()
  const isDisabled = user?.isSystemAdmin === true ? disabled : true
  const effectiveValue = user?.isSystemAdmin === true
    ? value
    : value || (user?.organizationId ?? '')

  useEffect(() => {
    if (user?.isSystemAdmin === true || value.length > 0 || user?.organizationId === undefined) {
      return
    }

    onValueChange(user.organizationId)
  }, [onValueChange, user?.isSystemAdmin, user?.organizationId, value])

  const loadOptions = useCallback(async (request: { search: string, selectedId: string }) => {
    return await getOrganizationLookup({
      id: request.selectedId,
      includeInactive,
      search: request.search,
      take: 25,
    })
  }, [includeInactive])
  const getOptionDescription = useCallback((organization: OrganizationLookupDto) => organization.code, [])
  const getOptionValue = useCallback((organization: OrganizationLookupDto) => organization.id, [])

  return (
    <AsyncLookupSelect
      cacheScope={`organizations:${includeInactive}`}
      clearLabel={t('shared.actions.clear')}
      clearable={clearable}
      disabled={isDisabled}
      getOptionDescription={getOptionDescription}
      getOptionLabel={getOptionLabel}
      getOptionValue={getOptionValue}
      loadOptions={loadOptions}
      onError={(error) => notifyError(error, t('shared.errors.generic'))}
      onValueChange={onValueChange}
      placeholder={t('resources.iam.organizations.placeholders.search')}
      value={effectiveValue}
    />
  )
}
