import { useCallback, useEffect, useMemo, useState } from 'react'

import { useTranslate } from '../../../app/i18n/i18n'
import { useNotifyError } from '../../../auth/AuthProvider'
import { Card, CardContent } from '../../../components/ui/card'
import { Field, FieldLabel } from '../../../components/ui/form'
import { getOrganizationSettingsParameters, getUserSettingsParameters } from './parameterApi'
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
  const [parameters, setParameters] = useState<ParameterLiteDto[]>([])
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
      setValues(Object.fromEntries(loaded.map((parameter) => [parameter.id, parameter.value])))
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    } finally {
      setIsLoading(false)
    }
  }, [notifyError, owner, t])

  useEffect(() => {
    void loadParameters()
  }, [loadParameters])

  return (
    <main className="page">
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
                          <FieldLabel htmlFor={`parameter-${parameter.id}`}>{parameter.title}</FieldLabel>
                          <ParameterValueInput
                            id={`parameter-${parameter.id}`}
                            onBlur={() => undefined}
                            onChange={(value) => setValues((current) => ({
                              ...current,
                              [parameter.id]: value,
                            }))}
                            type={String(parameter.type)}
                            value={values[parameter.id] ?? ''}
                          />
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
