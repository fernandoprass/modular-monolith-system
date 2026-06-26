import { RotateCcw, Save, Trash2 } from 'lucide-react'
import { useCallback, useEffect, useMemo, useState } from 'react'

import { useToast } from '../../../app/ToastProvider'
import { useTranslate } from '../../../app/i18n/i18n'
import { useNotifyError } from '../../../auth/AuthProvider'
import { Button } from '../../../components/ui/button'
import { Card, CardContent } from '../../../components/ui/card'
import { Field, FieldLabel } from '../../../components/ui/form'
import {
  deleteParameterOverride,
  getOrganizationSettingsParameters,
  getUserSettingsParameters,
  saveParameterOverride,
} from './parameterApi'
import { ParameterValueInput } from './ParameterValueInput'
import type { ParameterLiteDto } from './parameterTypes'

type ParameterSettingsOwner = 'organization' | 'user'

type ParameterSettingsPageProps = {
  owner: ParameterSettingsOwner
}

type GroupedParameters = Array<{
  groups: Array<{
    group: string
    parameters: ParameterLiteDto[]
  }>
  module: string
}>

function groupParameters(parameters: ParameterLiteDto[]): GroupedParameters {
  const byModule = new Map<string, Map<string, ParameterLiteDto[]>>()

  for (const parameter of parameters) {
    const moduleGroup = byModule.get(parameter.module) ?? new Map<string, ParameterLiteDto[]>()
    const groupItems = moduleGroup.get(parameter.group) ?? []

    groupItems.push(parameter)
    moduleGroup.set(parameter.group, groupItems)
    byModule.set(parameter.module, moduleGroup)
  }

  return [...byModule.entries()]
    .sort(([left], [right]) => left.localeCompare(right))
    .map(([module, groups]) => ({
      groups: [...groups.entries()]
        .sort(([left], [right]) => left.localeCompare(right))
        .map(([group, groupParameters]) => ({
          group,
          parameters: groupParameters.sort((left, right) => left.title.localeCompare(right.title)),
        })),
      module,
    }))
}

function getPageTitleKey(owner: ParameterSettingsOwner): string {
  return owner === 'organization'
    ? 'features.iam.parameters.pages.organizationSettings'
    : 'features.iam.parameters.pages.userSettings'
}

export function ParameterSettingsPage({ owner }: ParameterSettingsPageProps) {
  const t = useTranslate()
  const notifyError = useNotifyError()
  const { showSuccess } = useToast()
  const [parameters, setParameters] = useState<ParameterLiteDto[]>([])
  const [originalValues, setOriginalValues] = useState<Record<string, string>>({})
  const [savingParameterIds, setSavingParameterIds] = useState<Set<string>>(new Set())
  const [values, setValues] = useState<Record<string, string>>({})
  const [isLoading, setIsLoading] = useState(false)
  const groupedParameters = useMemo(() => groupParameters(parameters), [parameters])

  const loadParameters = useCallback(async () => {
    setIsLoading(true)

    try {
      const loaded = owner === 'organization'
        ? await getOrganizationSettingsParameters()
        : await getUserSettingsParameters()

      setParameters(loaded)
      const loadedValues = Object.fromEntries(loaded.map((parameter) => [parameter.id, parameter.value]))
      setOriginalValues(loadedValues)
      setValues(loadedValues)
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    } finally {
      setIsLoading(false)
    }
  }, [notifyError, owner, t])

  useEffect(() => {
    void loadParameters()
  }, [loadParameters])

  function setParameterSaving(parameterId: string, isSaving: boolean) {
    setSavingParameterIds((current) => {
      const next = new Set(current)

      if (isSaving) {
        next.add(parameterId)
      } else {
        next.delete(parameterId)
      }

      return next
    })
  }

  async function handleSaveOverride(parameter: ParameterLiteDto) {
    const value = values[parameter.id] ?? ''

    setParameterSaving(parameter.id, true)

    try {
      await saveParameterOverride(parameter.id, value)
      showSuccess(t('features.iam.parameters.notifications.overrideSaved'))
      await loadParameters()
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    } finally {
      setParameterSaving(parameter.id, false)
    }
  }

  async function handleDeleteOverride(parameter: ParameterLiteDto) {
    if (parameter.parameterOverrideId === null) {
      return
    }

    setParameterSaving(parameter.id, true)

    try {
      await deleteParameterOverride(parameter.parameterOverrideId)
      showSuccess(t('features.iam.parameters.notifications.overrideRemoved'))
      await loadParameters()
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    } finally {
      setParameterSaving(parameter.id, false)
    }
  }

  function handleReset(parameterId: string) {
    setValues((current) => ({
      ...current,
      [parameterId]: originalValues[parameterId] ?? '',
    }))
  }

  return (
    <main className="page settings-page">
      <h1 className="page-title">{t(getPageTitleKey(owner))}</h1>
      {isLoading && <p className="page-subtitle">{t('shared.common.loading')}</p>}
      {!isLoading && parameters.length === 0 && (
        <p className="page-subtitle">{t('features.iam.parameters.messages.empty')}</p>
      )}
      <div className="settings-stack">
        {groupedParameters.map((module) => (
          <section className="settings-module" key={module.module}>
            <h2 className="settings-module-title">{module.module}</h2>
            <div className="settings-group-grid">
              {module.groups.map((group) => (
                <Card key={`${module.module}.${group.group}`}>
                  <CardContent>
                    <h3 className="card-title">{group.group}</h3>
                    <div className="settings-parameter-list">
                      {group.parameters.map((parameter) => (
                        <Field className="settings-parameter-row" key={parameter.id}>
                          <div className="settings-parameter-copy">
                            <FieldLabel htmlFor={`parameter-${parameter.id}`}>{parameter.title}</FieldLabel>
                            {parameter.description.length > 0 && (
                              <p className="settings-parameter-description">{parameter.description}</p>
                            )}
                          </div>
                          <ParameterValueInput
                            disabled={savingParameterIds.has(parameter.id)}
                            id={`parameter-${parameter.id}`}
                            onBlur={() => undefined}
                            onChange={(value) => setValues((current) => ({
                              ...current,
                              [parameter.id]: value,
                            }))}
                            type={String(parameter.type)}
                            value={values[parameter.id] ?? ''}
                          />
                          <div className="settings-parameter-actions">
                            <Button
                              aria-label={t('shared.actions.saveOverride')}
                              disabled={savingParameterIds.has(parameter.id) || values[parameter.id] === originalValues[parameter.id]}
                              onClick={() => void handleSaveOverride(parameter)}
                              size="icon"
                              title={t('shared.actions.saveOverride')}
                              type="button"
                            >
                              <Save />
                            </Button>
                            <Button
                              aria-label={t('shared.actions.reset')}
                              disabled={savingParameterIds.has(parameter.id) || values[parameter.id] === originalValues[parameter.id]}
                              onClick={() => handleReset(parameter.id)}
                              size="icon"
                              title={t('shared.actions.reset')}
                              type="button"
                              variant="outline"
                            >
                              <RotateCcw />
                            </Button>
                            <Button
                              aria-label={t('shared.actions.removeOverride')}
                              disabled={savingParameterIds.has(parameter.id) || parameter.parameterOverrideId === null}
                              onClick={() => void handleDeleteOverride(parameter)}
                              size="icon"
                              title={t('shared.actions.removeOverride')}
                              type="button"
                              variant="outline"
                            >
                              <Trash2 />
                            </Button>
                          </div>
                        </Field>
                      ))}
                    </div>
                  </CardContent>
                </Card>
              ))}
            </div>
          </section>
        ))}
      </div>
    </main>
  )
}
