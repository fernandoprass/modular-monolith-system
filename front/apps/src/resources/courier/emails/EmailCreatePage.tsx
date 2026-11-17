import { toTranslatedOptions } from '../../../shared/translatedOptions'
import { zodResolver } from '@hookform/resolvers/zod'
import { createValidationErrorMap } from '../../../shared/validation'
import { ArrowLeft } from 'lucide-react'
import { useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { useNavigate } from 'react-router-dom'
import { z } from 'zod'

import { useToast } from '../../../app/ToastProvider'
import { useTranslate } from '../../../app/i18n/i18n'
import { APP_ROUTES } from '../../../app/routes'
import { useAuth, useNotifyError } from '../../../auth/AuthProvider'
import { Button } from '../../../components/ui/button'
import { Card, CardContent } from '../../../components/ui/card'
import { Checkbox } from '../../../components/ui/checkbox'
import { Field, FieldError, FieldGroup, FieldLabel } from '../../../components/ui/form'
import { Input } from '../../../components/ui/input'
import { Select } from '../../../components/ui/select'
import { Textarea } from '../../../components/ui/textarea'
import { OrganizationSelect } from '../../iam/organizations/OrganizationSelect'
import { UserSelect } from '../../iam/users/UserSelect'
import { createEmail } from './emailApi'
import {
  EMAIL_FEATURE_OPTIONS,
  EMAIL_MODULE_OPTIONS,
  type EmailCreateForm,
} from './emailTypes'

const emailCreateSchema = z.object({
  body: z.string().trim().min(1),
  feature: z.string().trim().min(2),
  isHtml: z.boolean(),
  module: z.string().trim().min(2),
  organizationId: z.string().trim().min(1),
  recipient: z.string().trim().email(),
  subject: z.string().trim().min(2),
  templateKey: z.string().trim().min(2),
  userId: z.string().trim().min(1),
})

export function EmailCreatePage() {
  const t = useTranslate()
  const navigate = useNavigate()
  const notifyError = useNotifyError()
  const { showSuccess } = useToast()
  const { user } = useAuth()
  const [isSaving, setIsSaving] = useState(false)
  const form = useForm<EmailCreateForm>({
    defaultValues: {
      body: '',
      feature: EMAIL_FEATURE_OPTIONS[0].value,
      isHtml: true,
      module: EMAIL_MODULE_OPTIONS[0].value,
      organizationId: user?.organizationId ?? '',
      recipient: '',
      subject: '',
      templateKey: '',
      userId: user?.id ?? '',
    },
    resolver: zodResolver(emailCreateSchema, { error: createValidationErrorMap(t) }),
  })

  async function handleSave(value: EmailCreateForm) {
    setIsSaving(true)

    try {
      const { data: created, message } = await createEmail(value)

      showSuccess(message)
      navigate(APP_ROUTES.emailView(created.id))
    } catch (error) {
      notifyError(error, t('common.messages.errors.generic'))
    } finally {
      setIsSaving(false)
    }
  }

  return (
    <main className="page courier-email-page">
      <div className="page-header">
        <h1 className="page-title">{t('common.pages.create', { entity: t('common.labels.email') })}</h1>
        <Button onClick={() => navigate(APP_ROUTES.emails)} type="button" variant="outline">
          <ArrowLeft data-icon="inline-start" />
          {t('common.actions.back')}
        </Button>
      </div>
      <Card>
        <CardContent>
          <form noValidate className="edit-form" onSubmit={form.handleSubmit(handleSave)}>
            <FieldGroup>
              <div className="form-row-two">
                <Controller
                  control={form.control}
                  name="organizationId"
                  render={({ field }) => (
                    <Field data-invalid={form.formState.errors.organizationId !== undefined}>
                      <FieldLabel htmlFor="EmailCreatePage-organizationId">{t('features.iam.labels.organization')}</FieldLabel>
                      <OrganizationSelect
                        onBlur={field.onBlur}
                        ref={field.ref}
                        aria-invalid={form.formState.errors.organizationId !== undefined}
                        aria-describedby={form.formState.errors.organizationId ? 'EmailCreatePage-organizationId-error' : undefined}
                        id="EmailCreatePage-organizationId"
                        onValueChange={field.onChange}
                        value={field.value}
                      />
                      <FieldError id="EmailCreatePage-organizationId-error">{form.formState.errors.organizationId?.message}</FieldError>
                    </Field>
                  )}
                />
                <Controller
                  control={form.control}
                  name="userId"
                  render={({ field }) => (
                    <Field data-invalid={form.formState.errors.userId !== undefined}>
                      <FieldLabel htmlFor="EmailCreatePage-userId">{t('common.labels.user')}</FieldLabel>
                      <UserSelect
                        onBlur={field.onBlur}
                        ref={field.ref}
                        aria-invalid={form.formState.errors.userId !== undefined}
                        aria-describedby={form.formState.errors.userId ? 'EmailCreatePage-userId-error' : undefined}
                        id="EmailCreatePage-userId"
                        onValueChange={field.onChange}
                        value={field.value}
                      />
                      <FieldError id="EmailCreatePage-userId-error">{form.formState.errors.userId?.message}</FieldError>
                    </Field>
                  )}
                />
              </div>
              <div className="form-row-two">
                <Controller
                  control={form.control}
                  name="module"
                  render={({ field }) => (
                    <Field data-invalid={form.formState.errors.module !== undefined}>
                      <FieldLabel htmlFor="EmailCreatePage-module">{t('common.labels.module')}</FieldLabel>
                      <Select
                        onBlur={field.onBlur}
                        ref={field.ref}
                        aria-invalid={form.formState.errors.module !== undefined}
                        aria-describedby={form.formState.errors.module ? 'EmailCreatePage-module-error' : undefined}
                        id="EmailCreatePage-module"
                        onValueChange={field.onChange}
                        options={toTranslatedOptions(EMAIL_MODULE_OPTIONS, t)}
                        value={field.value}
                      />
                      <FieldError id="EmailCreatePage-module-error">{form.formState.errors.module?.message}</FieldError>
                    </Field>
                  )}
                />
                <Controller
                  control={form.control}
                  name="feature"
                  render={({ field }) => (
                    <Field data-invalid={form.formState.errors.feature !== undefined}>
                      <FieldLabel htmlFor="EmailCreatePage-feature">{t('common.labels.feature')}</FieldLabel>
                      <Select
                        onBlur={field.onBlur}
                        ref={field.ref}
                        aria-invalid={form.formState.errors.feature !== undefined}
                        aria-describedby={form.formState.errors.feature ? 'EmailCreatePage-feature-error' : undefined}
                        id="EmailCreatePage-feature"
                        onValueChange={field.onChange}
                        options={toTranslatedOptions(EMAIL_FEATURE_OPTIONS, t)}
                        value={field.value}
                      />
                      <FieldError id="EmailCreatePage-feature-error">{form.formState.errors.feature?.message}</FieldError>
                    </Field>
                  )}
                />
              </div>
              <div className="form-row-two">
                <Field data-invalid={form.formState.errors.templateKey !== undefined}>
                  <FieldLabel htmlFor="email-template-key">{t('features.courier.labels.templateKey')}</FieldLabel>
                  <Input
                    aria-invalid={form.formState.errors.templateKey !== undefined}
                    aria-describedby={form.formState.errors.templateKey ? 'EmailCreatePage-templateKey-error' : undefined}
                    id="email-template-key"
                    required
                    {...form.register('templateKey')}
                  />
                  <FieldError id="EmailCreatePage-templateKey-error">{form.formState.errors.templateKey?.message}</FieldError>
                </Field>
                <Field data-invalid={form.formState.errors.recipient !== undefined}>
                  <FieldLabel htmlFor="email-recipient">{t('features.courier.labels.recipient')}</FieldLabel>
                  <Input
                    aria-invalid={form.formState.errors.recipient !== undefined}
                    aria-describedby={form.formState.errors.recipient ? 'EmailCreatePage-recipient-error' : undefined}
                    id="email-recipient"
                    required
                    type="email"
                    {...form.register('recipient')}
                  />
                  <FieldError id="EmailCreatePage-recipient-error">{form.formState.errors.recipient?.message}</FieldError>
                </Field>
              </div>
              <Field data-invalid={form.formState.errors.subject !== undefined}>
                <FieldLabel htmlFor="email-subject">{t('features.courier.labels.subject')}</FieldLabel>
                <Input
                  aria-invalid={form.formState.errors.subject !== undefined}
                  aria-describedby={form.formState.errors.subject ? 'EmailCreatePage-subject-error' : undefined}
                  id="email-subject"
                  required
                  {...form.register('subject')}
                />
                <FieldError id="EmailCreatePage-subject-error">{form.formState.errors.subject?.message}</FieldError>
              </Field>
              <Field data-invalid={form.formState.errors.body !== undefined}>
                <FieldLabel htmlFor="email-body">{t('features.courier.labels.body')}</FieldLabel>
                <Textarea
                  aria-invalid={form.formState.errors.body !== undefined}
                  aria-describedby={form.formState.errors.body ? 'EmailCreatePage-body-error' : undefined}
                  className="email-body-input"
                  id="email-body"
                  required
                  {...form.register('body')}
                />
                <FieldError id="EmailCreatePage-body-error">{form.formState.errors.body?.message}</FieldError>
              </Field>
              <Controller
                control={form.control}
                name="isHtml"
                render={({ field }) => (
                  <Checkbox
                    checked={field.value}
                    label={t('features.courier.labels.isHtml')}
                    onCheckedChange={field.onChange}
                  />
                )}
              />
            </FieldGroup>
            <div className="form-actions">
              <Button disabled={isSaving} type="submit">
                {t('common.actions.create')}
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </main>
  )
}
