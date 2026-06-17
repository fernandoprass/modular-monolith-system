import { useForm, useStore } from '@tanstack/react-form'
import { useState } from 'react'
import { Link } from 'react-router-dom'

import { APP_CONSTANTS } from '../../../app/appConstants'
import { useToast } from '../../../app/ToastProvider'
import { useTranslate } from '../../../app/i18n/i18n'
import { APP_ROUTES } from '../../../app/routes'
import { useNotifyError } from '../../../auth/AuthProvider'
import { Button } from '../../../components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../../../components/ui/card'
import { Field, FieldGroup, FieldLabel } from '../../../components/ui/form'
import { Input } from '../../../components/ui/input'
import { Select } from '../../../components/ui/select'
import { Textarea } from '../../../components/ui/textarea'
import { LANGUAGE_CODES, LANGUAGE_OPTIONS } from '../../../shared/languages'
import { ORGANIZATION_TYPES, ORGANIZATION_TYPE_OPTIONS, type OrganizationCreateForm } from './organizationTypes'
import { createOrganization } from './organizationApi'
import { toTranslatedOptions } from './organizationUi'

export function PublicOrganizationCreatePage() {
  const t = useTranslate()
  const notifyError = useNotifyError()
  const { showSuccess } = useToast()
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [isSuccess, setIsSuccess] = useState(false)
  const form = useForm({
    defaultValues: {
      code: '',
      defaultLanguage: LANGUAGE_CODES.english,
      description: '',
      name: '',
      type: ORGANIZATION_TYPES.company,
      userEmail: '',
      userName: '',
      userPassword: '',
    } as OrganizationCreateForm,
    onSubmit: async ({ value }) => {
      setIsSubmitting(true)

      try {
        await createOrganization(value)
        setIsSuccess(true)
        showSuccess(t('public.organizationRegistration.messages.success'))
      } catch (error) {
        notifyError(error, t('shared.errors.generic'))
      } finally {
        setIsSubmitting(false)
      }
    },
  })
  const isCompany = useStore(form.store, (state) => state.values.type === ORGANIZATION_TYPES.company)

  return (
    <main className="auth-page">
      <Card className="register-card">
        <CardHeader>
          <CardTitle>{APP_CONSTANTS.appName}</CardTitle>
          <CardDescription>{t('public.organizationRegistration.title')}</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="form-stack">
            {isSuccess ? (
              <>
                <p className="success-text">{t('public.organizationRegistration.messages.success')}</p>
                <Link className="text-link" to={APP_ROUTES.login}>
                  {t('public.organizationRegistration.actions.signIn')}
                </Link>
              </>
            ) : (
              <form onSubmit={(event) => {
                event.preventDefault()
                void form.handleSubmit()
              }}>
                <FieldGroup>
                  <form.Field name="type">
                    {(field) => (
                      <Field>
                        <FieldLabel>{t('public.organizationRegistration.fields.type')}</FieldLabel>
                        <Select
                          onValueChange={(value) => field.handleChange(Number(value))}
                          options={toTranslatedOptions(ORGANIZATION_TYPE_OPTIONS, t)}
                          value={String(field.state.value)}
                        />
                      </Field>
                    )}
                  </form.Field>
                  {isCompany && (
                    <>
                      <form.Field name="code">
                        {(field) => (
                          <Field>
                            <FieldLabel htmlFor={field.name}>{t('public.organizationRegistration.fields.code')}</FieldLabel>
                            <Input id={field.name} onBlur={field.handleBlur} onChange={(event) => field.handleChange(event.currentTarget.value)} required value={field.state.value} />
                          </Field>
                        )}
                      </form.Field>
                      <form.Field name="name">
                        {(field) => (
                          <Field>
                            <FieldLabel htmlFor={field.name}>{t('public.organizationRegistration.fields.name')}</FieldLabel>
                            <Input id={field.name} onBlur={field.handleBlur} onChange={(event) => field.handleChange(event.currentTarget.value)} required value={field.state.value} />
                          </Field>
                        )}
                      </form.Field>
                    </>
                  )}
                  <form.Field name="description">
                    {(field) => (
                      <Field>
                        <FieldLabel htmlFor={field.name}>{t('public.organizationRegistration.fields.description')}</FieldLabel>
                        <Textarea id={field.name} onBlur={field.handleBlur} onChange={(event) => field.handleChange(event.currentTarget.value)} required value={field.state.value} />
                      </Field>
                    )}
                  </form.Field>
                  <form.Field name="defaultLanguage">
                    {(field) => (
                      <Field>
                        <FieldLabel>{t('public.organizationRegistration.fields.defaultLanguage')}</FieldLabel>
                        <Select
                          onValueChange={field.handleChange}
                          options={toTranslatedOptions(LANGUAGE_OPTIONS, t)}
                          value={field.state.value}
                        />
                      </Field>
                    )}
                  </form.Field>
                  <form.Field name="userName">
                    {(field) => (
                      <Field>
                        <FieldLabel htmlFor={field.name}>{t('public.organizationRegistration.fields.adminName')}</FieldLabel>
                        <Input id={field.name} onBlur={field.handleBlur} onChange={(event) => field.handleChange(event.currentTarget.value)} required value={field.state.value} />
                      </Field>
                    )}
                  </form.Field>
                  <form.Field name="userEmail">
                    {(field) => (
                      <Field>
                        <FieldLabel htmlFor={field.name}>{t('public.organizationRegistration.fields.adminEmail')}</FieldLabel>
                        <Input id={field.name} onBlur={field.handleBlur} onChange={(event) => field.handleChange(event.currentTarget.value)} required type="email" value={field.state.value} />
                      </Field>
                    )}
                  </form.Field>
                  <form.Field name="userPassword">
                    {(field) => (
                      <Field>
                        <FieldLabel htmlFor={field.name}>{t('public.organizationRegistration.fields.adminPassword')}</FieldLabel>
                        <Input id={field.name} onBlur={field.handleBlur} onChange={(event) => field.handleChange(event.currentTarget.value)} required type="password" value={field.state.value} />
                      </Field>
                    )}
                  </form.Field>
                  <Button disabled={isSubmitting} type="submit">
                    {t('public.organizationRegistration.actions.submit')}
                  </Button>
                  <Link className="text-link text-center" to={APP_ROUTES.login}>
                    {t('public.organizationRegistration.actions.signIn')}
                  </Link>
                </FieldGroup>
              </form>
            )}
          </div>
        </CardContent>
      </Card>
    </main>
  )
}
