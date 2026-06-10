import { ChevronDown, X } from 'lucide-react'
import { useEffect, useMemo, useState } from 'react'

import { useTranslate } from '../../../app/i18n/i18n'
import { useAuth, useNotifyError } from '../../../auth/AuthProvider'
import { Button } from '../../../components/ui/button'
import { Input } from '../../../components/ui/input'
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
  const [search, setSearch] = useState('')
  const [options, setOptions] = useState<OrganizationLookupDto[]>([])
  const [selected, setSelected] = useState<OrganizationLookupDto | null>(null)
  const [isFocused, setIsFocused] = useState(false)

  const visibleOptions = useMemo(() => {
    if (selected === null) {
      return options
    }

    return options.some((option) => option.id === selected.id)
      ? options
      : [selected, ...options]
  }, [options, selected])

  useEffect(() => {
    let isCurrent = true

    async function loadOptions() {
      try {
        const loaded = await getOrganizationLookup({
          id: value,
          includeInactive,
          search,
          take: 25,
        })

        if (!isCurrent) {
          return
        }

        setOptions(loaded)

        const selectedOption = loaded.find((option) => option.id === value) ?? null
        if (selectedOption !== null) {
          setSelected(selectedOption)

          if (search.length === 0) {
            setSearch(getOptionLabel(selectedOption))
          }
        }
      } catch (error) {
        notifyError(error, t('shared.errors.generic'))
      }
    }

    void loadOptions()

    return () => {
      isCurrent = false
    }
  }, [includeInactive, notifyError, search, t, value])

  function handleSelect(organization: OrganizationLookupDto) {
    setSelected(organization)
    setSearch(getOptionLabel(organization))
    setIsFocused(false)
    onValueChange(organization.id)
  }

  function handleClear() {
    setSelected(null)
    setSearch('')
    onValueChange('')
  }

  const showOptions = isFocused && visibleOptions.length > 0

  return (
    <div className="lookup-select">
      <div className="lookup-input-row">
        <div className="lookup-input-wrap">
          <Input
            disabled={isDisabled}
            onChange={(event) => {
              if (isDisabled) {
                return
              }

              if (value.length > 0) {
                setSelected(null)
                onValueChange('')
              }

              setSearch(event.currentTarget.value)
              setIsFocused(true)
            }}
            onBlur={() => setIsFocused(false)}
            onFocus={() => !isDisabled && setIsFocused(true)}
            placeholder={t('resources.iam.organizations.placeholders.search')}
            value={search}
          />
          <ChevronDown className="lookup-chevron" size={14} />
        </div>
        {clearable && !isDisabled && value.length > 0 && (
          <Button onClick={handleClear} size="icon" title={t('shared.actions.clear')} type="button" variant="outline">
            <X size={15} />
          </Button>
        )}
      </div>
      {showOptions && (
        <div className="lookup-options">
          {visibleOptions.map((organization) => (
            <button
              className="lookup-option"
              key={organization.id}
              onMouseDown={(event) => event.preventDefault()}
              onClick={() => handleSelect(organization)}
              type="button"
            >
              <span>{organization.name}</span>
              <span>{organization.code}</span>
            </button>
          ))}
        </div>
      )}
    </div>
  )
}
