import { zodResolver } from '@hookform/resolvers/zod'
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
import { Field, FieldGroup, FieldLabel } from '../../../components/ui/form'
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
import { toTranslatedEmailOptions } from './emailUi'

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
    resolver: zodResolver(emailCreateSchema),
  })

  async function handleSave(value: EmailCreateForm) {
    setIsSaving(true)

    try {
      const created = await createEmail(value)

      showSuccess(t('features.courier.emails.notifications.created'))
      navigate(APP_ROUTES.emailView(created.id))
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    } finally {
      setIsSaving(false)
    }
  }

  return (
    <main className="page courier-email-page">
      <div className="page-header">
        <h1 className="page-title">{t('features.courier.emails.pages.create')}</h1>
        <Button onClick={() => navigate(APP_ROUTES.emails)} type="button" variant="outline">
          <ArrowLeft data-icon="inline-start" />
          {t('shared.actions.back')}
        </Button>
      </div>
      <Card>
        <CardContent>
          <form className="edit-form" onSubmit={form.handleSubmit(handleSave)}>
            <FieldGroup>
              <div className="form-row-two">
                <Controller
                  control={form.control}
                  name="organizationId"
                  render={({ field }) => (
                    <Field>
                      <FieldLabel>{t('shared.fields.organization')}</FieldLabel>
                      <OrganizationSelect onValueChange={field.onChange} value={field.value} />
                    </Field>
                  )}
                />
                <Controller
                  control={form.control}
                  name="userId"
                  render={({ field }) => (
                    <Field>
                      <FieldLabel>{t('shared.fields.user')}</FieldLabel>
                      <UserSelect onValueChange={field.onChange} value={field.value} />
                    </Field>
                  )}
                />
              </div>
              <div className="form-row-two">
                <Controller
                  control={form.control}
                  name="module"
                  render={({ field }) => (
                    <Field>
                      <FieldLabel>{t('shared.fields.module')}</FieldLabel>
                      <Select
                        onValueChange={field.onChange}
                        options={toTranslatedEmailOptions(EMAIL_MODULE_OPTIONS, t)}
                        value={field.value}
                      />
                    </Field>
                  )}
                />
                <Controller
                  control={form.control}
                  name="feature"
                  render={({ field }) => (
                    <Field>
                      <FieldLabel>{t('shared.fields.feature')}</FieldLabel>
                      <Select
                        onValueChange={field.onChange}
                        options={toTranslatedEmailOptions(EMAIL_FEATURE_OPTIONS, t)}
                        value={field.value}
                      />
                    </Field>
                  )}
                />
              </div>
              <div className="form-row-two">
                <Field>
                  <FieldLabel htmlFor="email-template-key">{t('shared.fields.templateKey')}</FieldLabel>
                  <Input id="email-template-key" required {...form.register('templateKey')} />
                </Field>
                <Field>
                  <FieldLabel htmlFor="email-recipient">{t('shared.fields.recipient')}</FieldLabel>
                  <Input id="email-recipient" required type="email" {...form.register('recipient')} />
                </Field>
              </div>
              <Field>
                <FieldLabel htmlFor="email-subject">{t('shared.fields.subject')}</FieldLabel>
                <Input id="email-subject" required {...form.register('subject')} />
              </Field>
              <Field>
                <FieldLabel htmlFor="email-body">{t('shared.fields.body')}</FieldLabel>
                <Textarea className="email-body-input" id="email-body" required {...form.register('body')} />
              </Field>
              <Controller
                control={form.control}
                name="isHtml"
                render={({ field }) => (
                  <Checkbox
                    checked={field.value}
                    label={t('shared.fields.isHtml')}
                    onCheckedChange={field.onChange}
                  />
                )}
              />
            </FieldGroup>
            <div className="form-actions">
              <Button disabled={isSaving} type="submit">
                {t('shared.actions.create')}
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </main>
  )
}
