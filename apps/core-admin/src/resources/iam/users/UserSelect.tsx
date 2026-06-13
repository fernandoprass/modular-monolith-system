import { useEffect, useState } from 'react'

import { useTranslate } from '../../../app/i18n/i18n'
import { useAuth, useNotifyError } from '../../../auth/AuthProvider'
import {
  InputSelect,
  InputSelectTrigger,
} from '../../../components/ui/input-select'
import type { SelectOption } from '../../../types'
import { getUserLookup } from './userApi'

type UserSelectProps = {
  clearable?: boolean
  disabled?: boolean
  includeInactive?: boolean
  onValueChange: (value: string) => void
  organizationId?: string
  value: string
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
  const [search, setSearch] = useState('')
  const [options, setOptions] = useState<SelectOption[]>([])
  const effectiveOrganizationId = user?.isSystemAdmin === true
    ? organizationId
    : user?.organizationId ?? ''

  useEffect(() => {
    setSearch('')
  }, [effectiveOrganizationId])

  useEffect(() => {
    const timeoutId = window.setTimeout(() => {
      void loadUserOptions()
    }, 350)

    async function loadUserOptions() {
      try {
        const users = await getUserLookup({
          id: value,
          includeInactive,
          organizationId: effectiveOrganizationId,
          search,
          take: 25,
        })

        setOptions(users.map((lookupUser) => ({
          label: lookupUser.name,
          value: lookupUser.id,
        })))
      } catch (error) {
        notifyError(error, t('shared.errors.generic'))
      }
    }

    return () => window.clearTimeout(timeoutId)
  }, [effectiveOrganizationId, includeInactive, notifyError, search, t, value])

  return (
    <InputSelect
      clearable={clearable}
      disabled={disabled}
      onSearchChange={setSearch}
      onValueChange={onValueChange}
      options={options}
      placeholder={t('resources.iam.users.placeholders.search')}
      searchValue={search}
      value={value}
    >
      {(selectProps) => <InputSelectTrigger {...selectProps} />}
    </InputSelect>
  )
}
