import { toTranslatedOptions } from '../../../shared/translatedOptions'
import { zodResolver } from '@hookform/resolvers/zod'
import { createValidationErrorMap } from '../../../shared/validation'
import { useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { Link, useNavigate } from 'react-router-dom'
import { z } from 'zod'

import { APP_CONSTANTS } from '../../../app/appConstants'
import { useToast } from '../../../app/ToastProvider'
import { useTranslate } from '../../../app/i18n/i18n'
import { APP_ROUTES } from '../../../app/routes'
import { useNotifyError } from '../../../auth/AuthProvider'
import { Button } from '../../../components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../../../components/ui/card'
import { Field, FieldError, FieldGroup, FieldLabel } from '../../../components/ui/form'
import { Input } from '../../../components/ui/input'
import { Select } from '../../../components/ui/select'
import { Textarea } from '../../../components/ui/textarea'
import { LANGUAGE_CODES, LANGUAGE_OPTIONS } from '../../../shared/languages'
import { ORGANIZATION_TYPES, ORGANIZATION_TYPE_OPTIONS, type OrganizationCreateForm } from './organizationTypes'
import { createOrganization } from './organizationApi'

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
  const navigate = useNavigate()
  const notifyError = useNotifyError()
  const { showSuccess } = useToast()
  const [isSubmitting, setIsSubmitting] = useState(false)
  const {
    formState: { errors },
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
    resolver: zodResolver(publicOrganizationCreateSchema, { error: createValidationErrorMap(t) }),
  })
  const isCompany = watch('type') === ORGANIZATION_TYPES.company

  async function handleCreate(value: OrganizationCreateForm) {
    setIsSubmitting(true)

    try {
      const { message } = await createOrganization(value)
      showSuccess(message)
      navigate(APP_ROUTES.login, { replace: true })
    } catch (error) {
      notifyError(error, t('common.messages.errors.generic'))
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <main className="auth-page">
      <Card className="register-card">
        <CardHeader>
          <CardTitle>{APP_CONSTANTS.appName}</CardTitle>
          <CardDescription>{t('common.pages.create', { entity: t('features.iam.labels.organization') })}</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="form-stack">
            <form noValidate onSubmit={handleSubmit(handleCreate)}>
              <FieldGroup>
                <Controller
                  control={control}
                  name="type"
                  render={({ field }) => (
                    <Field data-invalid={errors.type !== undefined}>
                      <FieldLabel htmlFor="PublicOrganizationCreatePage-type">{t('common.labels.type')}</FieldLabel>
                      <Select
                        onBlur={field.onBlur}
                        ref={field.ref}
                        aria-invalid={errors.type !== undefined}
                        aria-describedby={errors.type ? 'PublicOrganizationCreatePage-type-error' : undefined}
                        id="PublicOrganizationCreatePage-type"
                        onValueChange={(value) => field.onChange(Number(value))}
                        options={toTranslatedOptions(ORGANIZATION_TYPE_OPTIONS, t)}
                        value={String(field.value)}
                      />
                      <FieldError id="PublicOrganizationCreatePage-type-error">{errors.type?.message}</FieldError>
                    </Field>
                  )}
                />
                {isCompany && (
                  <>
                    <Field data-invalid={errors.code !== undefined}>
                      <FieldLabel htmlFor="code">{t('common.labels.code')}</FieldLabel>
                      <Input
                        aria-invalid={errors.code !== undefined}
                        aria-describedby={errors.code ? 'PublicOrganizationCreatePage-code-error' : undefined}
                        id="code"
                        required
                        {...register('code')}
                      />
                      <FieldError id="PublicOrganizationCreatePage-code-error">{errors.code?.message}</FieldError>
                    </Field>
                    <Field data-invalid={errors.name !== undefined}>
                      <FieldLabel htmlFor="name">{t('common.labels.name')}</FieldLabel>
                      <Input
                        aria-invalid={errors.name !== undefined}
                        aria-describedby={errors.name ? 'PublicOrganizationCreatePage-name-error' : undefined}
                        id="name"
                        required
                        {...register('name')}
                      />
                      <FieldError id="PublicOrganizationCreatePage-name-error">{errors.name?.message}</FieldError>
                    </Field>
                  </>
                )}
                <Field data-invalid={errors.description !== undefined}>
                  <FieldLabel htmlFor="description">{t('common.labels.description')}</FieldLabel>
                  <Textarea
                    aria-invalid={errors.description !== undefined}
                    aria-describedby={errors.description ? 'PublicOrganizationCreatePage-description-error' : undefined}
                    id="description"
                    required
                    {...register('description')}
                  />
                  <FieldError id="PublicOrganizationCreatePage-description-error">{errors.description?.message}</FieldError>
                </Field>
                <Controller
                  control={control}
                  name="defaultLanguage"
                  render={({ field }) => (
                    <Field data-invalid={errors.defaultLanguage !== undefined}>
                      <FieldLabel htmlFor="PublicOrganizationCreatePage-defaultLanguage">{t('common.labels.defaultLanguage')}</FieldLabel>
                      <Select
                        onBlur={field.onBlur}
                        ref={field.ref}
                        aria-invalid={errors.defaultLanguage !== undefined}
                        aria-describedby={errors.defaultLanguage ? 'PublicOrganizationCreatePage-defaultLanguage-error' : undefined}
                        id="PublicOrganizationCreatePage-defaultLanguage"
                        onValueChange={field.onChange}
                        options={toTranslatedOptions(LANGUAGE_OPTIONS, t)}
                        value={field.value}
                      />
                      <FieldError id="PublicOrganizationCreatePage-defaultLanguage-error">{errors.defaultLanguage?.message}</FieldError>
                    </Field>
                  )}
                />
                <Field data-invalid={errors.userName !== undefined}>
                  <FieldLabel htmlFor="userName">{t('features.iam.labels.adminName')}</FieldLabel>
                  <Input
                    aria-invalid={errors.userName !== undefined}
                    aria-describedby={errors.userName ? 'PublicOrganizationCreatePage-userName-error' : undefined}
                    id="userName"
                    required
                    {...register('userName')}
                  />
                  <FieldError id="PublicOrganizationCreatePage-userName-error">{errors.userName?.message}</FieldError>
                </Field>
                <Field data-invalid={errors.userEmail !== undefined}>
                  <FieldLabel htmlFor="userEmail">{t('features.iam.labels.adminEmail')}</FieldLabel>
                  <Input
                    aria-invalid={errors.userEmail !== undefined}
                    aria-describedby={errors.userEmail ? 'PublicOrganizationCreatePage-userEmail-error' : undefined}
                    id="userEmail"
                    required
                    type="email"
                    {...register('userEmail')}
                  />
                  <FieldError id="PublicOrganizationCreatePage-userEmail-error">{errors.userEmail?.message}</FieldError>
                </Field>
                <Field data-invalid={errors.userPassword !== undefined}>
                  <FieldLabel htmlFor="userPassword">{t('features.iam.labels.adminPassword')}</FieldLabel>
                  <Input
                    aria-invalid={errors.userPassword !== undefined}
                    aria-describedby={errors.userPassword ? 'PublicOrganizationCreatePage-userPassword-error' : undefined}
                    id="userPassword"
                    required
                    type="password"
                    {...register('userPassword')}
                  />
                  <FieldError id="PublicOrganizationCreatePage-userPassword-error">{errors.userPassword?.message}</FieldError>
                </Field>
                <Button disabled={isSubmitting} type="submit">
                  {t('common.pages.create', { entity: t('features.iam.labels.organization') })}
                </Button>
                <Link className="text-link text-center" to={APP_ROUTES.login}>
                  {t('features.iam.actions.backToSignIn')}
                </Link>
              </FieldGroup>
            </form>
          </div>
        </CardContent>
      </Card>
    </main>
  )
}
