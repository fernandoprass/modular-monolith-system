import { zodResolver } from '@hookform/resolvers/zod'
import { ArrowLeft, Edit } from 'lucide-react'
import { useCallback, useEffect, useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { useNavigate, useParams } from 'react-router-dom'
import { z } from 'zod'

import { useToast } from '../../../app/ToastProvider'
import { useTranslate } from '../../../app/i18n/i18n'
import { APP_ROUTES } from '../../../app/routes'
import { useNotifyError } from '../../../auth/AuthProvider'
import { Button } from '../../../components/ui/button'
import { Card, CardContent } from '../../../components/ui/card'
import { Checkbox } from '../../../components/ui/checkbox'
import { Field, FieldGroup, FieldLabel } from '../../../components/ui/form'
import { Input } from '../../../components/ui/input'
import { Select } from '../../../components/ui/select'
import { Textarea } from '../../../components/ui/textarea'
import { LANGUAGE_OPTIONS } from '../../../shared/languages'
import { OrganizationCodeEditDialog } from './OrganizationCodeEditDialog'
import { getOrganization, updateOrganization } from './organizationApi'
import { ORGANIZATION_REQUEST_FIELDS, type OrganizationDto } from './organizationTypes'
import { toTranslatedOptions } from './organizationUi'

type OrganizationEditForm = {
  defaultLanguage: string
  description: string
  isActive: boolean
  name: string
}

const organizationEditSchema = z.object({
  defaultLanguage: z.string().trim().min(1),
  description: z.string().trim().min(1),
  isActive: z.boolean(),
  name: z.string().trim().min(1),
})

function toForm(organization: OrganizationDto): OrganizationEditForm {
  return {
    defaultLanguage: organization.defaultLanguage,
    description: organization.description ?? '',
    isActive: organization.isActive,
    name: organization.name,
  }
}

export function OrganizationEditPage() {
  const t = useTranslate()
  const navigate = useNavigate()
  const notifyError = useNotifyError()
  const { id } = useParams()
  const [organization, setOrganization] = useState<OrganizationDto | null>(null)
  const [isCodeDialogOpen, setIsCodeDialogOpen] = useState(false)

  const loadOrganization = useCallback(async () => {
    if (id === undefined) {
      return
    }

    try {
      const loaded = await getOrganization(id)
      setOrganization(loaded)
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    }
  }, [id, notifyError, t])

  useEffect(() => {
    void loadOrganization()
  }, [loadOrganization])

  return (
    <main className="page">
      <div className="page-header">
        <h1 className="page-title">{t('features.iam.organizations.pages.edit')}</h1>
        <Button onClick={() => navigate(APP_ROUTES.organizations)} type="button" variant="outline">
          <ArrowLeft data-icon="inline-start" />
          {t('shared.actions.back')}
        </Button>
      </div>
      <Card>
        <CardContent>
          {organization === null ? (
            <p className="page-subtitle">{t('shared.common.loading')}</p>
          ) : (
            <OrganizationEditForm
              key={organization.id}
              onEditCode={() => setIsCodeDialogOpen(true)}
              onSaved={loadOrganization}
              organization={organization}
            />
          )}
        </CardContent>
      </Card>
      {organization !== null && (
        <OrganizationCodeEditDialog
          isOpen={isCodeDialogOpen}
          onClose={() => setIsCodeDialogOpen(false)}
          onSaved={loadOrganization}
          organization={organization}
        />
      )}
    </main>
  )
}

type OrganizationEditFormProps = {
  onEditCode: () => void
  onSaved: () => Promise<void>
  organization: OrganizationDto
}

function OrganizationEditForm({
  onEditCode,
  onSaved,
  organization,
}: OrganizationEditFormProps) {
  const t = useTranslate()
  const notifyError = useNotifyError()
  const { showSuccess } = useToast()
  const [isSaving, setIsSaving] = useState(false)
  const {
    control,
    handleSubmit,
    register,
  } = useForm<OrganizationEditForm>({
    defaultValues: toForm(organization),
    resolver: zodResolver(organizationEditSchema),
  })

  async function handleSave(value: OrganizationEditForm) {
    setIsSaving(true)

    try {
      await updateOrganization(organization.id, {
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
        <Field className="code-row" data-disabled>
          <div className="grow-field">
            <FieldLabel>{t('shared.fields.code')}</FieldLabel>
            <Input disabled value={organization.code} />
          </div>
          <Button onClick={onEditCode} type="button" variant="outline">
            <Edit data-icon="inline-start" />
            {t('shared.actions.editCode')}
          </Button>
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
        <Controller
          control={control}
          name="isActive"
          render={({ field }) => (
            <Checkbox
              checked={field.value}
              label={t('shared.fields.isActive')}
              onCheckedChange={field.onChange}
            />
          )}
        />
      </FieldGroup>
      <div className="form-actions">
        <Button disabled={isSaving} type="submit">{t('shared.actions.save')}</Button>
      </div>
    </form>
  )
}
