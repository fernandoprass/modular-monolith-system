import { useForm, useStore } from '@tanstack/react-form'
import { ArrowLeft } from 'lucide-react'
import { useCallback, useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'

import { useToast } from '../../../app/ToastProvider'
import { useTranslate, type Translate } from '../../../app/i18n/i18n'
import { APP_ROUTES } from '../../../app/routes'
import { useNotifyError } from '../../../auth/AuthProvider'
import { Button } from '../../../components/ui/button'
import { Card, CardContent } from '../../../components/ui/card'
import { Checkbox } from '../../../components/ui/checkbox'
import { Field, FieldGroup, FieldLabel } from '../../../components/ui/form'
import { Input } from '../../../components/ui/input'
import { Select } from '../../../components/ui/select'
import { Textarea } from '../../../components/ui/textarea'
import { getParameter, updateParameter } from './parameterApi'
import {
  PARAMETER_MODULE_OPTIONS,
  PARAMETER_OVERRIDE_TYPE_OPTIONS,
  PARAMETER_TYPE_OPTIONS,
  PARAMETER_TYPE_VALUES,
  type ParameterDto,
  type ParameterForm,
} from './parameterTypes'
import { ParameterValueInput } from './ParameterValueInput'

const EMPTY_PARAMETER_FORM: ParameterForm = {
  description: '',
  externalListEndpoint: '',
  group: '',
  isVisible: true,
  listItems: '',
  module: '',
  name: '',
  overrideType: '0',
  title: '',
  type: '8',
  value: '',
}

type ParameterOption = {
  labelKey: string
  value: string
}

function toTranslatedOptions(options: readonly ParameterOption[], t: Translate) {
  return options.map((option) => ({
    label: t(option.labelKey),
    value: option.value,
  }))
}

function toForm(parameter: ParameterDto): ParameterForm {
  return {
    description: parameter.description,
    externalListEndpoint: parameter.externalListEndpoint ?? '',
    group: parameter.group,
    isVisible: parameter.isVisible,
    listItems: parameter.listItems ?? '',
    module: parameter.module,
    name: parameter.name,
    overrideType: String(parameter.overrideType),
    title: parameter.title,
    type: String(parameter.type),
    value: parameter.value,
  }
}

function toSubmitForm(data: ParameterForm): ParameterForm {
  return {
    ...data,
    externalListEndpoint: data.type === PARAMETER_TYPE_VALUES.referenceId ? data.externalListEndpoint : '',
    listItems: data.type === PARAMETER_TYPE_VALUES.list ? data.listItems : '',
  }
}

export function ParameterEditPage() {
  const t = useTranslate()
  const navigate = useNavigate()
  const notifyError = useNotifyError()
  const { showSuccess } = useToast()
  const { id } = useParams()
  const [parameter, setParameter] = useState<ParameterDto | null>(null)
  const [isSaving, setIsSaving] = useState(false)
  const form = useForm({
    defaultValues: EMPTY_PARAMETER_FORM,
    onSubmit: async ({ value }) => {
      if (id === undefined) {
        return
      }

      setIsSaving(true)

      try {
        await updateParameter(id, toSubmitForm(value))
        showSuccess(t('features.iam.parameters.notifications.updated'))
        await loadParameter()
      } catch (error) {
        notifyError(error, t('shared.errors.generic'))
      } finally {
        setIsSaving(false)
      }
    },
  })

  const loadParameter = useCallback(async () => {
    if (id === undefined) {
      return
    }

    try {
      const loaded = await getParameter(id)
      setParameter(loaded)
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    }
  }, [id, notifyError, t])

  useEffect(() => {
    void loadParameter()
  }, [loadParameter])

  useEffect(() => {
    if (parameter === null) {
      return
    }

    form.reset(toForm(parameter))
  }, [form, parameter])

  const selectedType = useStore(form.store, (state) => state.values.type)
  const showListItems = selectedType === PARAMETER_TYPE_VALUES.list
  const showExternalListEndpoint = selectedType === PARAMETER_TYPE_VALUES.referenceId

  return (
    <main className="page">
      <div className="page-header">
        <h1 className="page-title">{t('features.iam.parameters.pages.edit')}</h1>
        <Button onClick={() => navigate(APP_ROUTES.parameters)} type="button" variant="outline">
          <ArrowLeft data-icon="inline-start" />
          {t('shared.actions.back')}
        </Button>
      </div>
      <Card>
        <CardContent>
          {parameter === null ? (
            <p className="page-subtitle">{t('shared.common.loading')}</p>
          ) : (
            <form className="edit-form" onSubmit={(event) => {
              event.preventDefault()
              void form.handleSubmit()
            }}>
              <FieldGroup>
                <Field>
                  <FieldLabel htmlFor="parameter-key">{t('shared.fields.key')}</FieldLabel>
                  <Input disabled id="parameter-key" value={parameter.key} />
                </Field>
                <div className="form-row-two">
                  <form.Field name="module">
                    {(field) => (
                      <Field>
                        <FieldLabel>{t('shared.fields.module')}</FieldLabel>
                        <Select
                          onValueChange={field.handleChange}
                          options={toTranslatedOptions(PARAMETER_MODULE_OPTIONS, t)}
                          value={field.state.value}
                        />
                      </Field>
                    )}
                  </form.Field>
                  <form.Field name="group">
                    {(field) => (
                      <Field>
                        <FieldLabel htmlFor={field.name}>{t('shared.fields.group')}</FieldLabel>
                        <Input id={field.name} onBlur={field.handleBlur} onChange={(event) => field.handleChange(event.currentTarget.value)} required value={field.state.value} />
                      </Field>
                    )}
                  </form.Field>
                </div>
                <form.Field name="name">
                  {(field) => (
                    <Field>
                      <FieldLabel htmlFor={field.name}>{t('shared.fields.name')}</FieldLabel>
                      <Input id={field.name} onBlur={field.handleBlur} onChange={(event) => field.handleChange(event.currentTarget.value)} required value={field.state.value} />
                    </Field>
                  )}
                </form.Field>
                <form.Field name="title">
                  {(field) => (
                    <Field>
                      <FieldLabel htmlFor={field.name}>{t('shared.fields.title')}</FieldLabel>
                      <Input id={field.name} onBlur={field.handleBlur} onChange={(event) => field.handleChange(event.currentTarget.value)} required value={field.state.value} />
                    </Field>
                  )}
                </form.Field>
                <form.Field name="description">
                  {(field) => (
                    <Field>
                      <FieldLabel htmlFor={field.name}>{t('shared.fields.description')}</FieldLabel>
                      <Textarea id={field.name} onBlur={field.handleBlur} onChange={(event) => field.handleChange(event.currentTarget.value)} required value={field.state.value} />
                    </Field>
                  )}
                </form.Field>
                <div className="form-row-two">
                  <form.Field name="overrideType">
                    {(field) => (
                      <Field>
                        <FieldLabel>{t('shared.fields.overrideType')}</FieldLabel>
                        <Select
                          onValueChange={field.handleChange}
                          options={toTranslatedOptions(PARAMETER_OVERRIDE_TYPE_OPTIONS, t)}
                          value={field.state.value}
                        />
                      </Field>
                    )}
                  </form.Field>
                  <form.Field name="type">
                    {(field) => (
                      <Field>
                        <FieldLabel>{t('shared.fields.type')}</FieldLabel>
                        <Select
                          onValueChange={field.handleChange}
                          options={toTranslatedOptions(PARAMETER_TYPE_OPTIONS, t)}
                          value={field.state.value}
                        />
                      </Field>
                    )}
                  </form.Field>
                </div>
                <form.Field name="value">
                  {(field) => (
                    <Field>
                      <FieldLabel htmlFor={field.name}>{t('shared.fields.value')}</FieldLabel>
                      <ParameterValueInput
                        id={field.name}
                        onBlur={field.handleBlur}
                        onChange={field.handleChange}
                        type={selectedType}
                        value={field.state.value}
                      />
                    </Field>
                  )}
                </form.Field>
                {showListItems && (
                  <form.Field name="listItems">
                    {(field) => (
                      <Field>
                        <FieldLabel htmlFor={field.name}>{t('shared.fields.listItems')}</FieldLabel>
                        <Textarea id={field.name} onBlur={field.handleBlur} onChange={(event) => field.handleChange(event.currentTarget.value)} required value={field.state.value} />
                      </Field>
                    )}
                  </form.Field>
                )}
                {showExternalListEndpoint && (
                  <form.Field name="externalListEndpoint">
                    {(field) => (
                      <Field>
                        <FieldLabel htmlFor={field.name}>{t('shared.fields.externalListEndpoint')}</FieldLabel>
                        <Input id={field.name} onBlur={field.handleBlur} onChange={(event) => field.handleChange(event.currentTarget.value)} required value={field.state.value} />
                      </Field>
                    )}
                  </form.Field>
                )}
                <form.Field name="isVisible">
                  {(field) => (
                    <Checkbox
                      checked={field.state.value}
                      label={t('shared.fields.isVisible')}
                      onCheckedChange={(checked) => field.handleChange(checked === true)}
                    />
                  )}
                </form.Field>
              </FieldGroup>
              <div className="form-actions">
                <Button disabled={isSaving} type="submit">
                  {t('shared.actions.save')}
                </Button>
              </div>
            </form>
          )}
        </CardContent>
      </Card>
    </main>
  )
}
