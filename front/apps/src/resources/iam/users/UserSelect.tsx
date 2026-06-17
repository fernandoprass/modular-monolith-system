import { useEffect, useState } from 'react'

import { useTranslate } from '../../../app/i18n/i18n'
import { useNotifyError } from '../../../auth/AuthProvider'
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
  value: string
}

export function UserSelect({
  clearable = false,
  disabled = false,
  includeInactive = false,
  onValueChange,
  value,
}: UserSelectProps) {
  const t = useTranslate()
  const notifyError = useNotifyError()
  const [search, setSearch] = useState('')
  const [options, setOptions] = useState<SelectOption[]>([])

  useEffect(() => {
    setSearch('')
  }, [value])

  useEffect(() => {
    const timeoutId = window.setTimeout(() => {
      void loadUserOptions()
    }, 350)

    async function loadUserOptions() {
      try {
        const users = await getUserLookup({
          id: value,
          includeInactive,
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
  }, [includeInactive, notifyError, search, t, value])

  return (
    <InputSelect
      clearable={clearable}
      disabled={disabled}
      onSearchChange={setSearch}
      onValueChange={onValueChange}
      options={options}
      placeholder={t('features.iam.users.placeholders.search')}
      searchValue={search}
      value={value}
    >
      {(selectProps) => <InputSelectTrigger {...selectProps} />}
    </InputSelect>
  )
}
