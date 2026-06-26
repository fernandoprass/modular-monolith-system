import { zodResolver } from '@hookform/resolvers/zod'
import { useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { Link } from 'react-router-dom'
import { z } from 'zod'

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

const publicOrganizationCreateSchema = z.object({
  code: z.string(),
  defaultLanguage: z.string().trim().min(1),
  description: z.string().trim().min(1),
  name: z.string(),
  type: z.number(),
  userEmail: z.string().trim().email(),
  userName: z.string().trim().min(1),
  userPassword: z.string().min(1),
})

export function PublicOrganizationCreatePage() {
  const t = useTranslate()
  const notifyError = useNotifyError()
  const { showSuccess } = useToast()
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [isSuccess, setIsSuccess] = useState(false)
  const {
    control,
    handleSubmit,
    register,
    watch,
  } = useForm<OrganizationCreateForm>({
    defaultValues: {
      code: '',
      defaultLanguage: LANGUAGE_CODES.english,
      description: '',
      name: '',
      type: ORGANIZATION_TYPES.company,
      userEmail: '',
      userName: '',
      userPassword: '',
    },
    resolver: zodResolver(publicOrganizationCreateSchema),
  })
  const isCompany = watch('type') === ORGANIZATION_TYPES.company

  async function handleCreate(value: OrganizationCreateForm) {
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
  }

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
              <form onSubmit={handleSubmit(handleCreate)}>
                <FieldGroup>
                  <Controller
                    control={control}
                    name="type"
                    render={({ field }) => (
                      <Field>
                        <FieldLabel>{t('public.organizationRegistration.fields.type')}</FieldLabel>
                        <Select
                          onValueChange={(value) => field.onChange(Number(value))}
                          options={toTranslatedOptions(ORGANIZATION_TYPE_OPTIONS, t)}
                          value={String(field.value)}
                        />
                      </Field>
                    )}
                  />
                  {isCompany && (
                    <>
                      <Field>
                        <FieldLabel htmlFor="code">{t('public.organizationRegistration.fields.code')}</FieldLabel>
                        <Input id="code" required {...register('code')} />
                      </Field>
                      <Field>
                        <FieldLabel htmlFor="name">{t('public.organizationRegistration.fields.name')}</FieldLabel>
                        <Input id="name" required {...register('name')} />
                      </Field>
                    </>
                  )}
                  <Field>
                    <FieldLabel htmlFor="description">{t('public.organizationRegistration.fields.description')}</FieldLabel>
                    <Textarea id="description" required {...register('description')} />
                  </Field>
                  <Controller
                    control={control}
                    name="defaultLanguage"
                    render={({ field }) => (
                      <Field>
                        <FieldLabel>{t('public.organizationRegistration.fields.defaultLanguage')}</FieldLabel>
                        <Select
                          onValueChange={field.onChange}
                          options={toTranslatedOptions(LANGUAGE_OPTIONS, t)}
                          value={field.value}
                        />
                      </Field>
                    )}
                  />
                  <Field>
                    <FieldLabel htmlFor="userName">{t('public.organizationRegistration.fields.adminName')}</FieldLabel>
                    <Input id="userName" required {...register('userName')} />
                  </Field>
                  <Field>
                    <FieldLabel htmlFor="userEmail">{t('public.organizationRegistration.fields.adminEmail')}</FieldLabel>
                    <Input id="userEmail" required type="email" {...register('userEmail')} />
                  </Field>
                  <Field>
                    <FieldLabel htmlFor="userPassword">{t('public.organizationRegistration.fields.adminPassword')}</FieldLabel>
                    <Input id="userPassword" required type="password" {...register('userPassword')} />
                  </Field>
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
