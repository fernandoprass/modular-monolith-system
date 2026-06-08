import { useState } from 'react'
import type { FormEvent } from 'react'
import { Link } from 'react-router-dom'

import { APP_CONSTANTS } from '../../../app/appConstants'
import { useToast } from '../../../app/ToastProvider'
import { useTranslate } from '../../../app/i18n/i18n'
import { APP_ROUTES } from '../../../app/routes'
import { useNotifyError } from '../../../auth/AuthProvider'
import { Button } from '../../../components/ui/button'
import { Card, CardContent } from '../../../components/ui/card'
import { Input } from '../../../components/ui/input'
import { Label } from '../../../components/ui/label'
import { Select } from '../../../components/ui/select'
import { LANGUAGE_CODES, LANGUAGE_OPTIONS } from '../../../shared/languages'
import { ORGANIZATION_TYPES, ORGANIZATION_TYPE_OPTIONS, type OrganizationCreateForm } from './organizationTypes'
import { createOrganization } from './organizationApi'
import { toTranslatedOptions } from './organizationUi'

export function PublicOrganizationCreatePage() {
  const t = useTranslate()
  const notifyError = useNotifyError()
  const { showSuccess } = useToast()
  const [form, setForm] = useState<OrganizationCreateForm>({
    code: '',
    defaultLanguage: LANGUAGE_CODES.english,
    description: '',
    name: '',
    type: ORGANIZATION_TYPES.company,
    userEmail: '',
    userName: '',
    userPassword: '',
  })
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [isSuccess, setIsSuccess] = useState(false)
  const isCompany = form.type === ORGANIZATION_TYPES.company

  function setField<TField extends keyof OrganizationCreateForm>(
    field: TField,
    value: OrganizationCreateForm[TField],
  ) {
    setForm((currentForm) => ({
      ...currentForm,
      [field]: value,
    }))
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setIsSubmitting(true)

    try {
      await createOrganization(form)
      setIsSuccess(true)
      showSuccess(t('public.organizationRegistration.messages.success'))
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <main className="auth-page">
      <Card className="register-card">
        <CardContent>
          <div className="form-stack">
            <div>
              <h1 className="auth-title">{APP_CONSTANTS.appName}</h1>
              <p className="auth-subtitle">{t('public.organizationRegistration.title')}</p>
            </div>
            {isSuccess ? (
              <>
                <p className="success-text">{t('public.organizationRegistration.messages.success')}</p>
                <Link className="text-link" to={APP_ROUTES.login}>
                  {t('public.organizationRegistration.actions.signIn')}
                </Link>
              </>
            ) : (
              <form onSubmit={handleSubmit}>
                <div className="form-stack">
                  <Field label={t('public.organizationRegistration.fields.type')}>
                    <Select
                      onValueChange={(value) => setField('type', Number(value))}
                      options={toTranslatedOptions(ORGANIZATION_TYPE_OPTIONS, t)}
                      value={String(form.type)}
                    />
                  </Field>
                  {isCompany && (
                    <>
                      <Field label={t('public.organizationRegistration.fields.code')}>
                        <Input onChange={(event) => setField('code', event.currentTarget.value)} required value={form.code} />
                      </Field>
                      <Field label={t('public.organizationRegistration.fields.name')}>
                        <Input onChange={(event) => setField('name', event.currentTarget.value)} required value={form.name} />
                      </Field>
                    </>
                  )}
                  <Field label={t('public.organizationRegistration.fields.description')}>
                    <Input onChange={(event) => setField('description', event.currentTarget.value)} required value={form.description} />
                  </Field>
                  <Field label={t('public.organizationRegistration.fields.defaultLanguage')}>
                    <Select
                      onValueChange={(value) => setField('defaultLanguage', value)}
                      options={toTranslatedOptions(LANGUAGE_OPTIONS, t)}
                      value={form.defaultLanguage}
                    />
                  </Field>
                  <Field label={t('public.organizationRegistration.fields.adminName')}>
                    <Input onChange={(event) => setField('userName', event.currentTarget.value)} required value={form.userName} />
                  </Field>
                  <Field label={t('public.organizationRegistration.fields.adminEmail')}>
                    <Input onChange={(event) => setField('userEmail', event.currentTarget.value)} required type="email" value={form.userEmail} />
                  </Field>
                  <Field label={t('public.organizationRegistration.fields.adminPassword')}>
                    <Input onChange={(event) => setField('userPassword', event.currentTarget.value)} required type="password" value={form.userPassword} />
                  </Field>
                  <Button disabled={isSubmitting} type="submit">
                    {t('public.organizationRegistration.actions.submit')}
                  </Button>
                  <Link className="text-link text-center" to={APP_ROUTES.login}>
                    {t('public.organizationRegistration.actions.signIn')}
                  </Link>
                </div>
              </form>
            )}
          </div>
        </CardContent>
      </Card>
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
