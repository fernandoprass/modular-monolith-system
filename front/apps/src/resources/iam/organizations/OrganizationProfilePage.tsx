import { zodResolver } from '@hookform/resolvers/zod'
import { useCallback, useEffect, useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { z } from 'zod'

import { useToast } from '../../../app/ToastProvider'
import { useTranslate } from '../../../app/i18n/i18n'
import { useNotifyError } from '../../../auth/AuthProvider'
import { Button } from '../../../components/ui/button'
import { Card, CardContent } from '../../../components/ui/card'
import { Field, FieldGroup, FieldLabel } from '../../../components/ui/form'
import { Input } from '../../../components/ui/input'
import { Select } from '../../../components/ui/select'
import { Textarea } from '../../../components/ui/textarea'
import { LANGUAGE_OPTIONS } from '../../../shared/languages'
import { getOwnOrganization, updateOwnOrganization } from './organizationApi'
import { ORGANIZATION_REQUEST_FIELDS, type OrganizationDto } from './organizationTypes'
import { toTranslatedOptions } from './organizationUi'

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
      notifyError(error, t('shared.errors.generic'))
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
            <p className="page-subtitle">{t('shared.common.loading')}</p>
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
    control,
    handleSubmit,
    register,
  } = useForm<OrganizationProfileForm>({
    defaultValues: toForm(organization),
    resolver: zodResolver(organizationProfileSchema),
  })

  async function handleSave(value: OrganizationProfileForm) {
    setIsSaving(true)

    try {
      await updateOwnOrganization({
        [ORGANIZATION_REQUEST_FIELDS.name]: value.name,
        [ORGANIZATION_REQUEST_FIELDS.description]: value.description,
        [ORGANIZATION_REQUEST_FIELDS.isActive]: value.isActive,
        [ORGANIZATION_REQUEST_FIELDS.defaultLanguage]: value.defaultLanguage,
      })
      showSuccess(t('features.iam.organizations.notifications.updated'))
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
        <Field data-disabled>
          <FieldLabel>{t('shared.fields.code')}</FieldLabel>
          <Input disabled value={organization.code} />
        </Field>
        <Field>
          <FieldLabel htmlFor="name">{t('shared.fields.name')}</FieldLabel>
          <Input id="name" required {...register('name')} />
        </Field>
        <Field>
          <FieldLabel htmlFor="description">{t('shared.fields.description')}</FieldLabel>
          <Textarea id="description" required {...register('description')} />
        </Field>
        <Controller
          control={control}
          name="defaultLanguage"
          render={({ field }) => (
            <Field>
              <FieldLabel>{t('shared.fields.defaultLanguage')}</FieldLabel>
              <Select
                onValueChange={field.onChange}
                options={toTranslatedOptions(LANGUAGE_OPTIONS, t)}
                value={field.value}
              />
            </Field>
          )}
        />
      </FieldGroup>
      <div className="form-actions">
        <Button disabled={isSaving} type="submit">{t('shared.actions.save')}</Button>
      </div>
    </form>
  )
}
