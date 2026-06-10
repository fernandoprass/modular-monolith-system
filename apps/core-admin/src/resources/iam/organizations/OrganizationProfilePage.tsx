import { useCallback, useEffect, useState } from 'react'
import type { FormEvent } from 'react'

import { useToast } from '../../../app/ToastProvider'
import { useTranslate } from '../../../app/i18n/i18n'
import { useNotifyError } from '../../../auth/AuthProvider'
import { Button } from '../../../components/ui/button'
import { Card, CardContent } from '../../../components/ui/card'
import { Checkbox } from '../../../components/ui/checkbox'
import { Field } from '../../../components/ui/field'
import { Input } from '../../../components/ui/input'
import { Label } from '../../../components/ui/label'
import { Select } from '../../../components/ui/select'
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
  const [form, setForm] = useState<OrganizationProfileForm>({
    defaultLanguage: LANGUAGE_CODES.english,
    description: '',
    isActive: true,
    name: '',
  })
  const [isSaving, setIsSaving] = useState(false)

  const loadOrganization = useCallback(async () => {
    try {
      const loaded = await getOwnOrganization()
      setOrganization(loaded)
      setForm({
        defaultLanguage: loaded.defaultLanguage,
        description: loaded.description ?? '',
        isActive: loaded.isActive,
        name: loaded.name,
      })
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    }
  }, [notifyError, t])

  useEffect(() => {
    void loadOrganization()
  }, [loadOrganization])

  function setField<TField extends keyof OrganizationProfileForm>(
    field: TField,
    value: OrganizationProfileForm[TField],
  ) {
    setForm((currentForm) => ({
      ...currentForm,
      [field]: value,
    }))
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setIsSaving(true)

    try {
      await updateOwnOrganization({
        [ORGANIZATION_REQUEST_FIELDS.name]: form.name,
        [ORGANIZATION_REQUEST_FIELDS.description]: form.description,
        [ORGANIZATION_REQUEST_FIELDS.isActive]: form.isActive,
        [ORGANIZATION_REQUEST_FIELDS.defaultLanguage]: form.defaultLanguage,
      })
      showSuccess(t('resources.iam.organizations.notifications.updated'))
      await loadOrganization()
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    } finally {
      setIsSaving(false)
    }
  }

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
            <form className="edit-form" onSubmit={handleSubmit}>
              <div className="field">
                <Label>{t('resources.iam.organizations.fields.code')}</Label>
                <Input disabled value={organization.code} />
              </div>
              <Field label={t('resources.iam.organizations.fields.name')}>
                <Input onChange={(event) => setField('name', event.currentTarget.value)} required value={form.name} />
              </Field>
              <Field label={t('resources.iam.organizations.fields.description')}>
                <Input
                  onChange={(event) => setField('description', event.currentTarget.value)}
                  required
                  value={form.description}
                />
              </Field>
              <Field label={t('resources.iam.organizations.fields.defaultLanguage')}>
                <Select
                  onValueChange={(value) => setField('defaultLanguage', value)}
                  options={toTranslatedOptions(LANGUAGE_OPTIONS, t)}
                  value={form.defaultLanguage}
                />
              </Field>
              <Checkbox
                checked={form.isActive}
                label={t('resources.iam.organizations.fields.isActive')}
                onCheckedChange={(checked) => setField('isActive', checked)}
              />
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
