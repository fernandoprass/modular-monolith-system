import { toTranslatedOptions } from '../../../shared/translatedOptions'
import { zodResolver } from '@hookform/resolvers/zod'
import { createValidationErrorMap } from '../../../shared/validation'
import { ArrowLeft } from 'lucide-react'
import { useCallback, useEffect, useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { useNavigate, useParams } from 'react-router-dom'

import { useToast } from '../../../app/ToastProvider'
import { useTranslate } from '../../../app/i18n/i18n'
import { APP_ROUTES } from '../../../app/routes'
import { useNotifyError } from '../../../auth/AuthProvider'
import { Button } from '../../../components/ui/button'
import { Card, CardContent } from '../../../components/ui/card'
import { Checkbox } from '../../../components/ui/checkbox'
import { Field, FieldError, FieldGroup, FieldLabel } from '../../../components/ui/form'
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
import { createParameterEditSchema } from './parameterValidation'

type ParameterEditFormProps = {
  onSaved: () => Promise<void>
  parameter: ParameterDto
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
      notifyError(error, t('common.errors.generic'))
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
          {t('common.actions.back')}
        </Button>
      </div>
      <Card>
        <CardContent>
          {parameter === null ? (
            <p className="page-subtitle">{t('common.states.loading')}</p>
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
    formState: { errors },
    control,
    handleSubmit,
    register,
    watch,
  } = useForm<ParameterForm>({
    defaultValues: toForm(parameter),
    resolver: zodResolver(createParameterEditSchema(t), { error: createValidationErrorMap(t) }),
  })
  const selectedType = watch('type')
  const showListItems = selectedType === PARAMETER_TYPE_VALUES.list
  const showExternalListEndpoint = selectedType === PARAMETER_TYPE_VALUES.referenceId

  async function handleSave(value: ParameterForm) {
    setIsSaving(true)

    try {
      const message = await updateParameter(parameter.id, toSubmitForm(value))
      showSuccess(message)
      await onSaved()
    } catch (error) {
      notifyError(error, t('common.errors.generic'))
    } finally {
      setIsSaving(false)
    }
  }

  return (
    <form noValidate className="edit-form" onSubmit={handleSubmit(handleSave)}>
      <FieldGroup>
        <Field>
          <FieldLabel htmlFor="parameter-key">{t('common.fields.key')}</FieldLabel>
          <Input disabled id="parameter-key" value={parameter.key} />
        </Field>
        <div className="form-row-two">
          <Controller
            control={control}
            name="module"
            render={({ field }) => (
              <Field data-invalid={errors.module !== undefined}>
                <FieldLabel htmlFor="ParameterEditPage-module">{t('common.fields.module')}</FieldLabel>
                <Select
                  onBlur={field.onBlur}
                  ref={field.ref}
                  aria-invalid={errors.module !== undefined}
                  aria-describedby={errors.module ? 'ParameterEditPage-module-error' : undefined}
                  id="ParameterEditPage-module"
                  onValueChange={field.onChange}
                  options={toTranslatedOptions(PARAMETER_MODULE_OPTIONS, t)}
                  value={field.value}
                />
                <FieldError id="ParameterEditPage-module-error">{errors.module?.message}</FieldError>
              </Field>
            )}
          />
          <Field data-invalid={errors.group !== undefined}>
            <FieldLabel htmlFor="group">{t('common.fields.group')}</FieldLabel>
            <Input
              aria-invalid={errors.group !== undefined}
              aria-describedby={errors.group ? 'ParameterEditPage-group-error' : undefined}
              id="group"
              required
              {...register('group')}
            />
            <FieldError id="ParameterEditPage-group-error">{errors.group?.message}</FieldError>
          </Field>
        </div>
        <Field data-invalid={errors.name !== undefined}>
          <FieldLabel htmlFor="name">{t('common.fields.name')}</FieldLabel>
          <Input
            aria-invalid={errors.name !== undefined}
            aria-describedby={errors.name ? 'ParameterEditPage-name-error' : undefined}
            id="name"
            required
            {...register('name')}
          />
          <FieldError id="ParameterEditPage-name-error">{errors.name?.message}</FieldError>
        </Field>
        <Field data-invalid={errors.title !== undefined}>
          <FieldLabel htmlFor="title">{t('common.fields.title')}</FieldLabel>
          <Input
            aria-invalid={errors.title !== undefined}
            aria-describedby={errors.title ? 'ParameterEditPage-title-error' : undefined}
            id="title"
            required
            {...register('title')}
          />
          <FieldError id="ParameterEditPage-title-error">{errors.title?.message}</FieldError>
        </Field>
        <Field data-invalid={errors.description !== undefined}>
          <FieldLabel htmlFor="description">{t('common.fields.description')}</FieldLabel>
          <Textarea
            aria-invalid={errors.description !== undefined}
            aria-describedby={errors.description ? 'ParameterEditPage-description-error' : undefined}
            id="description"
            required
            {...register('description')}
          />
          <FieldError id="ParameterEditPage-description-error">{errors.description?.message}</FieldError>
        </Field>
        <div className="form-row-two">
          <Controller
            control={control}
            name="overrideType"
            render={({ field }) => (
              <Field data-invalid={errors.overrideType !== undefined}>
                <FieldLabel htmlFor="ParameterEditPage-overrideType">{t('common.fields.overrideType')}</FieldLabel>
                <Select
                  onBlur={field.onBlur}
                  ref={field.ref}
                  aria-invalid={errors.overrideType !== undefined}
                  aria-describedby={errors.overrideType ? 'ParameterEditPage-overrideType-error' : undefined}
                  id="ParameterEditPage-overrideType"
                  onValueChange={field.onChange}
                  options={toTranslatedOptions(PARAMETER_OVERRIDE_TYPE_OPTIONS, t)}
                  value={field.value}
                />
                <FieldError id="ParameterEditPage-overrideType-error">{errors.overrideType?.message}</FieldError>
              </Field>
            )}
          />
          <Controller
            control={control}
            name="type"
            render={({ field }) => (
              <Field data-invalid={errors.type !== undefined}>
                <FieldLabel htmlFor="ParameterEditPage-type">{t('common.fields.type')}</FieldLabel>
                <Select
                  onBlur={field.onBlur}
                  ref={field.ref}
                  aria-invalid={errors.type !== undefined}
                  aria-describedby={errors.type ? 'ParameterEditPage-type-error' : undefined}
                  id="ParameterEditPage-type"
                  onValueChange={field.onChange}
                  options={toTranslatedOptions(PARAMETER_TYPE_OPTIONS, t)}
                  value={field.value}
                />
                <FieldError id="ParameterEditPage-type-error">{errors.type?.message}</FieldError>
              </Field>
            )}
          />
        </div>
        <Controller
          control={control}
          name="value"
          render={({ field }) => (
            <Field data-invalid={errors.value !== undefined}>
              <FieldLabel htmlFor="value">{t('common.fields.value')}</FieldLabel>
              <ParameterValueInput
                aria-invalid={errors.value !== undefined}
                aria-describedby={errors.value ? 'ParameterEditPage-value-error' : undefined}
                id="value"
                onBlur={field.onBlur}
                onChange={field.onChange}
                type={selectedType}
                value={field.value}
              />
              <FieldError id="ParameterEditPage-value-error">{errors.value?.message}</FieldError>
            </Field>
          )}
        />
        {showListItems && (
          <Field data-invalid={errors.listItems !== undefined}>
            <FieldLabel htmlFor="listItems">{t('common.fields.listItems')}</FieldLabel>
            <Textarea
              aria-invalid={errors.listItems !== undefined}
              aria-describedby={errors.listItems ? 'ParameterEditPage-listItems-error' : undefined}
              id="listItems"
              required
              {...register('listItems')}
            />
            <FieldError id="ParameterEditPage-listItems-error">{errors.listItems?.message}</FieldError>
          </Field>
        )}
        {showExternalListEndpoint && (
          <Field data-invalid={errors.externalListEndpoint !== undefined}>
            <FieldLabel htmlFor="externalListEndpoint">{t('common.fields.externalListEndpoint')}</FieldLabel>
            <Input
              aria-invalid={errors.externalListEndpoint !== undefined}
              aria-describedby={errors.externalListEndpoint ? 'ParameterEditPage-externalListEndpoint-error' : undefined}
              id="externalListEndpoint"
              required
              {...register('externalListEndpoint')}
            />
            <FieldError id="ParameterEditPage-externalListEndpoint-error">{errors.externalListEndpoint?.message}</FieldError>
          </Field>
        )}
        <Controller
          control={control}
          name="isVisible"
          render={({ field }) => (
            <Checkbox
              checked={field.value}
              label={t('common.fields.isVisible')}
              onCheckedChange={field.onChange}
            />
          )}
        />
      </FieldGroup>
      <div className="form-actions">
        <Button disabled={isSaving} type="submit">
          {t('common.actions.save')}
        </Button>
      </div>
    </form>
  )
}
