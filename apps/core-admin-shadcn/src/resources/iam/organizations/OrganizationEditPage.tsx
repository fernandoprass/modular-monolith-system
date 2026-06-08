import { ArrowLeft, Edit } from 'lucide-react'
import { useCallback, useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { useNavigate, useParams } from 'react-router-dom'

import { useToast } from '../../../app/ToastProvider'
import { useTranslate } from '../../../app/i18n/i18n'
import { APP_ROUTES } from '../../../app/routes'
import { useNotifyError } from '../../../auth/AuthProvider'
import { Button } from '../../../components/ui/button'
import { Card, CardContent } from '../../../components/ui/card'
import { Checkbox } from '../../../components/ui/checkbox'
import { Input } from '../../../components/ui/input'
import { Label } from '../../../components/ui/label'
import { Select } from '../../../components/ui/select'
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

export function OrganizationEditPage() {
  const t = useTranslate()
  const navigate = useNavigate()
  const notifyError = useNotifyError()
  const { showSuccess } = useToast()
  const { id } = useParams()
  const [organization, setOrganization] = useState<OrganizationDto | null>(null)
  const [form, setForm] = useState<OrganizationEditForm>({
    defaultLanguage: LANGUAGE_CODES.english,
    description: '',
    isActive: true,
    name: '',
  })
  const [isSaving, setIsSaving] = useState(false)
  const [isCodeDialogOpen, setIsCodeDialogOpen] = useState(false)

  const loadOrganization = useCallback(async () => {
    if (id === undefined) {
      return
    }

    try {
      const loaded = await getOrganization(id)
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
  }, [id, notifyError, t])

  useEffect(() => {
    void loadOrganization()
  }, [loadOrganization])

  function setField<TField extends keyof OrganizationEditForm>(
    field: TField,
    value: OrganizationEditForm[TField],
  ) {
    setForm((currentForm) => ({
      ...currentForm,
      [field]: value,
    }))
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    if (id === undefined) {
      return
    }

    setIsSaving(true)

    try {
      await updateOrganization(id, {
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
        <h1 className="page-title">{t('resources.iam.organizations.pages.edit')}</h1>
        <Button onClick={() => navigate(APP_ROUTES.organizations)} type="button" variant="outline">
          <ArrowLeft size={16} />
          {t('shared.actions.close')}
        </Button>
      </div>
      <Card>
        <CardContent>
          {organization === null ? (
            <p className="page-subtitle">{t('shared.common.loading')}</p>
          ) : (
            <form className="edit-form" onSubmit={handleSubmit}>
              <div className="field code-row">
                <div className="grow-field">
                  <Label>{t('resources.iam.organizations.fields.code')}</Label>
                  <Input disabled value={organization.code} />
                </div>
                <Button onClick={() => setIsCodeDialogOpen(true)} type="button" variant="outline">
                  <Edit size={16} />
                  {t('resources.iam.organizations.actions.editCode')}
                </Button>
              </div>
              <Field label={t('resources.iam.organizations.fields.name')}>
                <Input onChange={(event) => setField('name', event.currentTarget.value)} required value={form.name} />
              </Field>
              <Field label={t('resources.iam.organizations.fields.description')}>
                <Input onChange={(event) => setField('description', event.currentTarget.value)} required value={form.description} />
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

type FieldProps = {
  children: React.ReactNode
  label: string
}

function Field({ children, label }: FieldProps) {
  return (
    <div className="field">
      <Label>{label}</Label>
      {children}
    </div>
  )
}
