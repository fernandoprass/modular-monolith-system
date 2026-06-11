import { useForm } from '@tanstack/react-form'
import { useCallback, useEffect, useState } from 'react'

import { useToast } from '../../../app/ToastProvider'
import { useTranslate } from '../../../app/i18n/i18n'
import { useNotifyError } from '../../../auth/AuthProvider'
import { Button } from '../../../components/ui/button'
import { Card, CardContent } from '../../../components/ui/card'
import { Field, FieldGroup, FieldLabel } from '../../../components/ui/form'
import { Input } from '../../../components/ui/input'
import { Select } from '../../../components/ui/select'
import { Textarea } from '../../../components/ui/textarea'
import { LANGUAGE_CODES, LANGUAGE_OPTIONS } from '../../../shared/languages'
import { getOwnOrganization, updateOwnOrganization } from './organizationApi'
import { ORGANIZATION_REQUEST_FIELDS, type OrganizationDto } from './organizationTypes'
import { toTranslatedOptions } from './organizationUi'

type OrganizationProfileForm = {
  defaultLanguage: string
  description: string
  isActive: boolean
  name: string
}

export function OrganizationProfilePage() {
  const t = useTranslate()
  const notifyError = useNotifyError()
  const { showSuccess } = useToast()
  const [organization, setOrganization] = useState<OrganizationDto | null>(null)
  const [isSaving, setIsSaving] = useState(false)
  const form = useForm({
    defaultValues: {
      defaultLanguage: LANGUAGE_CODES.english,
      description: '',
      isActive: true,
      name: '',
    } as OrganizationProfileForm,
    onSubmit: async ({ value }) => {
      setIsSaving(true)

      try {
        await updateOwnOrganization({
          [ORGANIZATION_REQUEST_FIELDS.name]: value.name,
          [ORGANIZATION_REQUEST_FIELDS.description]: value.description,
          [ORGANIZATION_REQUEST_FIELDS.isActive]: value.isActive,
          [ORGANIZATION_REQUEST_FIELDS.defaultLanguage]: value.defaultLanguage,
        })
        showSuccess(t('resources.iam.organizations.notifications.updated'))
        await loadOrganization()
      } catch (error) {
        notifyError(error, t('shared.errors.generic'))
      } finally {
        setIsSaving(false)
      }
    },
  })

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

  useEffect(() => {
    if (organization === null) {
      return
    }

    form.reset({
      defaultLanguage: organization.defaultLanguage,
      description: organization.description ?? '',
      isActive: organization.isActive,
      name: organization.name,
    })
  }, [form, organization])

  return (
    <main className="page">
      <div className="page-header">
        <h1 className="page-title">{t('resources.iam.organizations.pages.profile')}</h1>
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
                <Field data-disabled>
                  <FieldLabel>{t('resources.iam.organizations.fields.code')}</FieldLabel>
                  <Input disabled value={organization.code} />
                </Field>
                <form.Field name="name">
                  {(field) => (
                    <Field>
                      <FieldLabel htmlFor={field.name}>{t('resources.iam.organizations.fields.name')}</FieldLabel>
                      <Input id={field.name} onBlur={field.handleBlur} onChange={(event) => field.handleChange(event.currentTarget.value)} required value={field.state.value} />
                    </Field>
                  )}
                </form.Field>
                <form.Field name="description">
                  {(field) => (
                    <Field>
                      <FieldLabel htmlFor={field.name}>{t('resources.iam.organizations.fields.description')}</FieldLabel>
                      <Textarea
                        id={field.name}
                        onBlur={field.handleBlur}
                        onChange={(event) => field.handleChange(event.currentTarget.value)}
                        required
                        value={field.state.value}
                      />
                    </Field>
                  )}
                </form.Field>
                <form.Field name="defaultLanguage">
                  {(field) => (
                    <Field>
                      <FieldLabel>{t('resources.iam.organizations.fields.defaultLanguage')}</FieldLabel>
                      <Select
                        onValueChange={field.handleChange}
                        options={toTranslatedOptions(LANGUAGE_OPTIONS, t)}
                        value={field.state.value}
                      />
                    </Field>
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
    </main>
  )
}
