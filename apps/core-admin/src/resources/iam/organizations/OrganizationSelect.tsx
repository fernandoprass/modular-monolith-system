import { useEffect, useState } from 'react'

import { useTranslate } from '../../../app/i18n/i18n'
import { useAuth, useNotifyError } from '../../../auth/AuthProvider'
import {
  InputSelect,
  InputSelectTrigger,
} from '../../../components/ui/input-select'
import type { SelectOption } from '../../../types'
import { getOrganizationLookup } from './organizationApi'

type OrganizationSelectProps = {
  clearable?: boolean
  disabled?: boolean
  includeInactive?: boolean
  onValueChange: (value: string) => void
  value: string
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
  const [search, setSearch] = useState('')
  const [options, setOptions] = useState<SelectOption[]>([])
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

  useEffect(() => {
    const timeoutId = window.setTimeout(() => {
      void loadOrganizationOptions()
    }, 350)

    async function loadOrganizationOptions() {
      try {
        const organizations = await getOrganizationLookup({
          id: effectiveValue,
          includeInactive,
          search,
          take: 25,
        })

        setOptions(organizations.map((organization) => ({
          label: `${organization.name} (${organization.code})`,
          value: organization.id,
        })))
      } catch (error) {
        notifyError(error, t('shared.errors.generic'))
      }
    }

    return () => window.clearTimeout(timeoutId)
  }, [effectiveValue, includeInactive, notifyError, search, t])

  return (
    <InputSelect
      clearable={clearable}
      disabled={isDisabled}
      onSearchChange={setSearch}
      onValueChange={onValueChange}
      options={options}
      placeholder={t('resources.iam.organizations.placeholders.search')}
      searchValue={search}
      value={effectiveValue}
    >
      {(selectProps) => <InputSelectTrigger {...selectProps} />}
    </InputSelect>
  )
}
