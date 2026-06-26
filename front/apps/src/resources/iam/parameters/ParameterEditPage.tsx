import { zodResolver } from '@hookform/resolvers/zod'
import { ArrowLeft } from 'lucide-react'
import { useCallback, useEffect, useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { useNavigate, useParams } from 'react-router-dom'
import { z } from 'zod'

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

type ParameterOption = {
  labelKey: string
  value: string
}

type ParameterEditFormProps = {
  onSaved: () => Promise<void>
  parameter: ParameterDto
}

function toTranslatedOptions(options: readonly ParameterOption[], t: Translate) {
  return options.map((option) => ({
    label: t(option.labelKey),
    value: option.value,
  }))
}

const parameterEditSchema = z.object({
  description: z.string().trim().min(1),
  externalListEndpoint: z.string(),
  group: z.string().trim().min(1),
  isVisible: z.boolean(),
  listItems: z.string(),
  module: z.string().trim().min(1),
  name: z.string().trim().min(1),
  overrideType: z.string().trim().min(1),
  title: z.string().trim().min(1),
  type: z.string().trim().min(1),
  value: z.string(),
})

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
  const { id } = useParams()
  const [parameter, setParameter] = useState<ParameterDto | null>(null)

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
    setParameter(null)
  }, [id])

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
            <ParameterEditForm key={parameter.id} onSaved={loadParameter} parameter={parameter} />
          )}
        </CardContent>
      </Card>
    </main>
  )
}

function ParameterEditForm({ onSaved, parameter }: ParameterEditFormProps) {
  const t = useTranslate()
  const notifyError = useNotifyError()
  const { showSuccess } = useToast()
  const [isSaving, setIsSaving] = useState(false)
  const {
    control,
    handleSubmit,
    register,
    watch,
  } = useForm<ParameterForm>({
    defaultValues: toForm(parameter),
    resolver: zodResolver(parameterEditSchema),
  })
  const selectedType = watch('type')
  const showListItems = selectedType === PARAMETER_TYPE_VALUES.list
  const showExternalListEndpoint = selectedType === PARAMETER_TYPE_VALUES.referenceId

  async function handleSave(value: ParameterForm) {
    setIsSaving(true)

    try {
      await updateParameter(parameter.id, toSubmitForm(value))
      showSuccess(t('features.iam.parameters.notifications.updated'))
      await onSaved()
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    } finally {
      setIsSaving(false)
    }
  }

  return (
    <form className="edit-form" onSubmit={handleSubmit(handleSave)}>
      <FieldGroup>
        <Field>
          <FieldLabel htmlFor="parameter-key">{t('shared.fields.key')}</FieldLabel>
          <Input disabled id="parameter-key" value={parameter.key} />
        </Field>
        <div className="form-row-two">
          <Controller
            control={control}
            name="module"
            render={({ field }) => (
              <Field>
                <FieldLabel>{t('shared.fields.module')}</FieldLabel>
                <Select
                  onValueChange={field.onChange}
                  options={toTranslatedOptions(PARAMETER_MODULE_OPTIONS, t)}
                  value={field.value}
                />
              </Field>
            )}
          />
          <Field>
            <FieldLabel htmlFor="group">{t('shared.fields.group')}</FieldLabel>
            <Input id="group" required {...register('group')} />
          </Field>
        </div>
        <Field>
          <FieldLabel htmlFor="name">{t('shared.fields.name')}</FieldLabel>
          <Input id="name" required {...register('name')} />
        </Field>
        <Field>
          <FieldLabel htmlFor="title">{t('shared.fields.title')}</FieldLabel>
          <Input id="title" required {...register('title')} />
        </Field>
        <Field>
          <FieldLabel htmlFor="description">{t('shared.fields.description')}</FieldLabel>
          <Textarea id="description" required {...register('description')} />
        </Field>
        <div className="form-row-two">
          <Controller
            control={control}
            name="overrideType"
            render={({ field }) => (
              <Field>
                <FieldLabel>{t('shared.fields.overrideType')}</FieldLabel>
                <Select
                  onValueChange={field.onChange}
                  options={toTranslatedOptions(PARAMETER_OVERRIDE_TYPE_OPTIONS, t)}
                  value={field.value}
                />
              </Field>
            )}
          />
          <Controller
            control={control}
            name="type"
            render={({ field }) => (
              <Field>
                <FieldLabel>{t('shared.fields.type')}</FieldLabel>
                <Select
                  onValueChange={field.onChange}
                  options={toTranslatedOptions(PARAMETER_TYPE_OPTIONS, t)}
                  value={field.value}
                />
              </Field>
            )}
          />
        </div>
        <Controller
          control={control}
          name="value"
          render={({ field }) => (
            <Field>
              <FieldLabel htmlFor="value">{t('shared.fields.value')}</FieldLabel>
              <ParameterValueInput
                id="value"
                onBlur={field.onBlur}
                onChange={field.onChange}
                type={selectedType}
                value={field.value}
              />
            </Field>
          )}
        />
        {showListItems && (
          <Field>
            <FieldLabel htmlFor="listItems">{t('shared.fields.listItems')}</FieldLabel>
            <Textarea id="listItems" required {...register('listItems')} />
          </Field>
        )}
        {showExternalListEndpoint && (
          <Field>
            <FieldLabel htmlFor="externalListEndpoint">{t('shared.fields.externalListEndpoint')}</FieldLabel>
            <Input id="externalListEndpoint" required {...register('externalListEndpoint')} />
          </Field>
        )}
        <Controller
          control={control}
          name="isVisible"
          render={({ field }) => (
            <Checkbox
              checked={field.value}
              label={t('shared.fields.isVisible')}
              onCheckedChange={field.onChange}
            />
          )}
        />
      </FieldGroup>
      <div className="form-actions">
        <Button disabled={isSaving} type="submit">
          {t('shared.actions.save')}
        </Button>
      </div>
    </form>
  )
}
