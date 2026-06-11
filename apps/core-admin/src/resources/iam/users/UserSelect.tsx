import { useCallback } from 'react'

import { useTranslate } from '../../../app/i18n/i18n'
import { useAuth, useNotifyError } from '../../../auth/AuthProvider'
import { AsyncLookupSelect } from '../../../components/ui/async-lookup-select'
import { getUserLookup } from './userApi'
import type { UserLookupDto } from './userTypes'

type UserSelectProps = {
  clearable?: boolean
  disabled?: boolean
  includeInactive?: boolean
  onValueChange: (value: string) => void
  organizationId?: string
  value: string
}

function getOptionLabel(user: UserLookupDto): string {
  return user.name
}

export function UserSelect({
  clearable = false,
  disabled = false,
  includeInactive = false,
  onValueChange,
  organizationId = '',
  value,
}: UserSelectProps) {
  const t = useTranslate()
  const { user } = useAuth()
  const notifyError = useNotifyError()
  const effectiveOrganizationId = user?.isSystemAdmin === true
    ? organizationId
    : user?.organizationId ?? ''
  const loadOptions = useCallback(async (request: { search: string, selectedId: string }) => {
    return await getUserLookup({
      id: request.selectedId,
      includeInactive,
      organizationId: effectiveOrganizationId,
      search: request.search,
      take: 25,
    })
  }, [effectiveOrganizationId, includeInactive])
  const getOptionValue = useCallback((lookupUser: UserLookupDto) => lookupUser.id, [])

  return (
    <AsyncLookupSelect
      cacheScope={`users:${effectiveOrganizationId}:${includeInactive}`}
      clearLabel={t('shared.actions.clear')}
      clearable={clearable}
      disabled={disabled}
      getOptionLabel={getOptionLabel}
      getOptionValue={getOptionValue}
      loadOptions={loadOptions}
      onError={(error) => notifyError(error, t('shared.errors.generic'))}
      onValueChange={onValueChange}
      placeholder={t('resources.iam.users.placeholders.search')}
      value={value}
    />
  )
}
