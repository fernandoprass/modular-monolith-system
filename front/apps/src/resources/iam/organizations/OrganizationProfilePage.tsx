import { toTranslatedOptions } from '../../../shared/translatedOptions'
import { zodResolver } from '@hookform/resolvers/zod'
import { createValidationErrorMap } from '../../../shared/validation'
import { useCallback, useEffect, useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { z } from 'zod'

import { useToast } from '../../../app/ToastProvider'
import { useTranslate } from '../../../app/i18n/i18n'
import { useNotifyError } from '../../../auth/AuthProvider'
import { Button } from '../../../components/ui/button'
import { Card, CardContent } from '../../../components/ui/card'
import { Field, FieldError, FieldGroup, FieldLabel } from '../../../components/ui/form'
import { Input } from '../../../components/ui/input'
import { Select } from '../../../components/ui/select'
import { Textarea } from '../../../components/ui/textarea'
import { LANGUAGE_OPTIONS } from '../../../shared/languages'
import { getOwnOrganization, updateOwnOrganization } from './organizationApi'
import { ORGANIZATION_REQUEST_FIELDS, type OrganizationDto } from './organizationTypes'

type OrganizationProfileForm = {
  defaultLanguage: string
  description: string
  isActive: boolean
  name: string
}

const organizationProfileSchema = z.object({
  defaultLanguage: z.string().trim().min(1),
  description: z.string().trim().min(1),
  isActive: z.boolean(),
  name: z.string().trim().min(1),
})

function toForm(organization: OrganizationDto): OrganizationProfileForm {
  return {
    defaultLanguage: organization.defaultLanguage,
    description: organization.description ?? '',
    isActive: organization.isActive,
    name: organization.name,
  }
}

export function OrganizationProfilePage() {
  const t = useTranslate()
  const notifyError = useNotifyError()
  const [organization, setOrganization] = useState<OrganizationDto | null>(null)

  const loadOrganization = useCallback(async () => {
    try {
      const loaded = await getOwnOrganization()
      setOrganization(loaded)
    } catch (error) {
      notifyError(error, t('common.errors.generic'))
    }
  }, [notifyError, t])

  useEffect(() => {
    void loadOrganization()
  }, [loadOrganization])

  return (
    <main className="page">
      <div className="page-header">
        <h1 className="page-title">{t('features.iam.organizations.pages.profile')}</h1>
      </div>
      <Card>
        <CardContent>
          {organization === null ? (
            <p className="page-subtitle">{t('common.states.loading')}</p>
          ) : (
            <OrganizationProfileFormPanel
              key={organization.id}
              onSaved={loadOrganization}
              organization={organization}
            />
          )}
        </CardContent>
      </Card>
    </main>
  )
}

type OrganizationProfileFormPanelProps = {
  onSaved: () => Promise<void>
  organization: OrganizationDto
}

function OrganizationProfileFormPanel({
  onSaved,
  organization,
}: OrganizationProfileFormPanelProps) {
  const t = useTranslate()
  const notifyError = useNotifyError()
  const { showSuccess } = useToast()
  const [isSaving, setIsSaving] = useState(false)
  const {
    formState: { errors },
    control,
    handleSubmit,
    register,
  } = useForm<OrganizationProfileForm>({
    defaultValues: toForm(organization),
    resolver: zodResolver(organizationProfileSchema, { error: createValidationErrorMap(t) }),
  })

  async function handleSave(value: OrganizationProfileForm) {
    setIsSaving(true)

    try {
      const message = await updateOwnOrganization({
        [ORGANIZATION_REQUEST_FIELDS.name]: value.name,
        [ORGANIZATION_REQUEST_FIELDS.description]: value.description,
        [ORGANIZATION_REQUEST_FIELDS.isActive]: value.isActive,
        [ORGANIZATION_REQUEST_FIELDS.defaultLanguage]: value.defaultLanguage,
      })
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
        <Field data-disabled>
          <FieldLabel>{t('common.fields.code')}</FieldLabel>
          <Input disabled value={organization.code} />
        </Field>
        <Field data-invalid={errors.name !== undefined}>
          <FieldLabel htmlFor="name">{t('common.fields.name')}</FieldLabel>
          <Input
            aria-invalid={errors.name !== undefined}
            aria-describedby={errors.name ? 'OrganizationProfilePage-name-error' : undefined}
            id="name"
            required
            {...register('name')}
          />
          <FieldError id="OrganizationProfilePage-name-error">{errors.name?.message}</FieldError>
        </Field>
        <Field data-invalid={errors.description !== undefined}>
          <FieldLabel htmlFor="description">{t('common.fields.description')}</FieldLabel>
          <Textarea
            aria-invalid={errors.description !== undefined}
            aria-describedby={errors.description ? 'OrganizationProfilePage-description-error' : undefined}
            id="description"
            required
            {...register('description')}
          />
          <FieldError id="OrganizationProfilePage-description-error">{errors.description?.message}</FieldError>
        </Field>
        <Controller
          control={control}
          name="defaultLanguage"
          render={({ field }) => (
            <Field data-invalid={errors.defaultLanguage !== undefined}>
              <FieldLabel htmlFor="OrganizationProfilePage-defaultLanguage">{t('common.fields.defaultLanguage')}</FieldLabel>
              <Select
                onBlur={field.onBlur}
                ref={field.ref}
                aria-invalid={errors.defaultLanguage !== undefined}
                aria-describedby={errors.defaultLanguage ? 'OrganizationProfilePage-defaultLanguage-error' : undefined}
                id="OrganizationProfilePage-defaultLanguage"
                onValueChange={field.onChange}
                options={toTranslatedOptions(LANGUAGE_OPTIONS, t)}
                value={field.value}
              />
              <FieldError id="OrganizationProfilePage-defaultLanguage-error">{errors.defaultLanguage?.message}</FieldError>
            </Field>
          )}
        />
      </FieldGroup>
      <div className="form-actions">
        <Button disabled={isSaving} type="submit">{t('common.actions.save')}</Button>
      </div>
    </form>
  )
}
