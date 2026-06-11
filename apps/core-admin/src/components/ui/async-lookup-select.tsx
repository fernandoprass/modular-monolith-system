import { ChevronDown, X } from 'lucide-react'
import { useEffect, useMemo, useRef, useState } from 'react'

import { Input } from './input'

type AsyncLookupSelectLoadRequest = {
  search: string
  selectedId: string
}

type AsyncLookupSelectProps<TItem> = {
  cacheScope?: string
  clearLabel: string
  clearable?: boolean
  disabled?: boolean
  getOptionDescription?: (item: TItem) => string
  getOptionLabel: (item: TItem) => string
  getOptionValue: (item: TItem) => string
  loadOptions: (request: AsyncLookupSelectLoadRequest) => Promise<TItem[]>
  minSearchLength?: number
  onError?: (error: unknown) => void
  onValueChange: (value: string) => void
  placeholder: string
  value: string
}

const LOOKUP_DEBOUNCE_MS = 350

export function AsyncLookupSelect<TItem>({
  cacheScope = 'default',
  clearLabel,
  clearable = false,
  disabled = false,
  getOptionDescription,
  getOptionLabel,
  getOptionValue,
  loadOptions,
  minSearchLength = 0,
  onError,
  onValueChange,
  placeholder,
  value,
}: AsyncLookupSelectProps<TItem>) {
  const cacheRef = useRef(new Map<string, TItem[]>())
  const [search, setSearch] = useState('')
  const [debouncedSearch, setDebouncedSearch] = useState('')
  const [options, setOptions] = useState<TItem[]>([])
  const [selected, setSelected] = useState<TItem | null>(null)
  const [isFocused, setIsFocused] = useState(false)

  useEffect(() => {
    const timeoutId = window.setTimeout(() => setDebouncedSearch(search), LOOKUP_DEBOUNCE_MS)

    return () => window.clearTimeout(timeoutId)
  }, [search])

  const visibleOptions = useMemo(() => {
    if (selected === null || value.length === 0) {
      return options
    }

    return options.some((option) => getOptionValue(option) === getOptionValue(selected))
      ? options
      : [selected, ...options]
  }, [getOptionValue, options, selected, value.length])

  useEffect(() => {
    if (!isFocused && value.length === 0) {
      return
    }

    if (value.length === 0 && debouncedSearch.trim().length < minSearchLength) {
      setOptions([])
      return
    }

    let isCurrent = true
    const cacheKey = `${cacheScope}:${value}:${debouncedSearch.trim()}`
    const cachedOptions = cacheRef.current.get(cacheKey)

    async function loadLookupOptions() {
      try {
        const loaded = cachedOptions ?? await loadOptions({
          search: debouncedSearch,
          selectedId: value,
        })

        if (!isCurrent) {
          return
        }

        cacheRef.current.set(cacheKey, loaded)
        setOptions(loaded)

        if (value.length > 0) {
          const selectedOption = loaded.find((option) => getOptionValue(option) === value) ?? null
          setSelected(selectedOption)

          if (selectedOption !== null && search.length === 0) {
            setSearch(getOptionLabel(selectedOption))
          }
        }
      } catch (error) {
        if (isCurrent) {
          onError?.(error)
        }
      }
    }

    void loadLookupOptions()

    return () => {
      isCurrent = false
    }
  }, [
    cacheScope,
    debouncedSearch,
    getOptionLabel,
    getOptionValue,
    isFocused,
    loadOptions,
    minSearchLength,
    onError,
    search.length,
    value,
  ])

  function handleSelect(option: TItem) {
    setSelected(option)
    setSearch(getOptionLabel(option))
    setIsFocused(false)
    onValueChange(getOptionValue(option))
  }

  function handleClear() {
    setSelected(null)
    setSearch('')
    setOptions([])
    onValueChange('')
  }

  const showOptions = isFocused && visibleOptions.length > 0

  return (
    <div className="lookup-select">
      <div className="lookup-input-wrap">
        <Input
          disabled={disabled}
          onBlur={() => setIsFocused(false)}
          onChange={(event) => {
            if (disabled) {
              return
            }

            if (value.length > 0) {
              setSelected(null)
              setOptions([])
              onValueChange('')
            }

            setSearch(event.currentTarget.value)
            setIsFocused(true)
          }}
          onFocus={() => !disabled && setIsFocused(true)}
          placeholder={placeholder}
          value={search}
        />
        {clearable && !disabled && value.length > 0 ? (
          <button
            aria-label={clearLabel}
            className="lookup-clear"
            onClick={handleClear}
            type="button"
          >
            <X data-icon="inline-start" />
          </button>
        ) : (
          <ChevronDown className="lookup-chevron" />
        )}
      </div>
      {showOptions && (
        <div className="lookup-options">
          {visibleOptions.map((option) => (
            <button
              className="lookup-option"
              key={getOptionValue(option)}
              onClick={() => handleSelect(option)}
              onMouseDown={(event) => event.preventDefault()}
              type="button"
            >
              <span>{getOptionLabel(option)}</span>
              {getOptionDescription !== undefined && <span>{getOptionDescription(option)}</span>}
            </button>
          ))}
        </div>
      )}
    </div>
  )
}
