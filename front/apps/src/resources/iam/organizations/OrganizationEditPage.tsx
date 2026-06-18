import { useForm } from '@tanstack/react-form'
import { ArrowLeft, Edit } from 'lucide-react'
import { useCallback, useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'

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
import { LANGUAGE_CODES, LANGUAGE_OPTIONS } from '../../../shared/languages'
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

const EMPTY_ORGANIZATION_EDIT_FORM: OrganizationEditForm = {
  defaultLanguage: LANGUAGE_CODES.english,
  description: '',
  isActive: true,
  name: '',
}

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
  const { showSuccess } = useToast()
  const { id } = useParams()
  const [organization, setOrganization] = useState<OrganizationDto | null>(null)
  const [isSaving, setIsSaving] = useState(false)
  const [isCodeDialogOpen, setIsCodeDialogOpen] = useState(false)
  const form = useForm({
    defaultValues: EMPTY_ORGANIZATION_EDIT_FORM,
    onSubmit: async ({ value }) => {
      if (id === undefined) {
        return
      }

      setIsSaving(true)

      try {
        await updateOrganization(id, {
          [ORGANIZATION_REQUEST_FIELDS.name]: value.name,
          [ORGANIZATION_REQUEST_FIELDS.description]: value.description,
          [ORGANIZATION_REQUEST_FIELDS.isActive]: value.isActive,
          [ORGANIZATION_REQUEST_FIELDS.defaultLanguage]: value.defaultLanguage,
        })
        showSuccess(t('features.iam.organizations.notifications.updated'))
        await loadOrganization()
      } catch (error) {
        notifyError(error, t('shared.errors.generic'))
      } finally {
        setIsSaving(false)
      }
    },
  })

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

  useEffect(() => {
    if (organization === null) {
      return
    }

    form.reset(toForm(organization))
  }, [form, organization])

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
            <form className="edit-form" onSubmit={(event) => {
              event.preventDefault()
              void form.handleSubmit()
            }}>
              <FieldGroup>
                <Field className="code-row" data-disabled>
                  <div className="grow-field">
                    <FieldLabel>{t('shared.fields.code')}</FieldLabel>
                    <Input disabled value={organization.code} />
                  </div>
                  <Button onClick={() => setIsCodeDialogOpen(true)} type="button" variant="outline">
                    <Edit data-icon="inline-start" />
                    {t('shared.actions.editCode')}
                  </Button>
                </Field>
                <form.Field name="name">
                  {(field) => (
                    <Field>
                      <FieldLabel htmlFor={field.name}>{t('shared.fields.name')}</FieldLabel>
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
                <form.Field name="defaultLanguage">
                  {(field) => (
                    <Field>
                      <FieldLabel>{t('shared.fields.defaultLanguage')}</FieldLabel>
                      <Select
                        onValueChange={field.handleChange}
                        options={toTranslatedOptions(LANGUAGE_OPTIONS, t)}
                        value={field.state.value}
                      />
                    </Field>
                  )}
                </form.Field>
                <form.Field name="isActive">
                  {(field) => (
                    <Checkbox
                      checked={field.state.value}
                      label={t('shared.fields.isActive')}
                      onCheckedChange={(checked) => field.handleChange(checked === true)}
                    />
                  )}
                </form.Field>
              </FieldGroup>
              <div className="form-actions">
                <Button disabled={isSaving} type="submit">{t('shared.actions.save')}</Button>
              </div>
            </form>
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
