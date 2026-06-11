import { useForm } from '@tanstack/react-form'
import { ArrowLeft } from 'lucide-react'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'

import { useToast } from '../../../app/ToastProvider'
import { useTranslate } from '../../../app/i18n/i18n'
import { APP_ROUTES } from '../../../app/routes'
import { useAuth, useNotifyError } from '../../../auth/AuthProvider'
import { Button } from '../../../components/ui/button'
import { Card, CardContent } from '../../../components/ui/card'
import { Field, FieldGroup, FieldLabel } from '../../../components/ui/form'
import { Input } from '../../../components/ui/input'
import { Select } from '../../../components/ui/select'
import { LANGUAGE_CODES, LANGUAGE_OPTIONS } from '../../../shared/languages'
import { OrganizationSelect } from '../organizations/OrganizationSelect'
import { createUser } from './userApi'
import type { UserCreateForm } from './userTypes'
import { toTranslatedOptions } from './userUi'

export function UserCreatePage() {
  const t = useTranslate()
  const navigate = useNavigate()
  const notifyError = useNotifyError()
  const { user } = useAuth()
  const { showError, showSuccess } = useToast()
  const [isSubmitting, setIsSubmitting] = useState(false)
  const form = useForm({
    defaultValues: {
      email: '',
      language: LANGUAGE_CODES.english,
      name: '',
      organizationId: user?.isSystemAdmin === true ? '' : user?.organizationId ?? '',
      password: '',
    } as UserCreateForm,
    onSubmit: async ({ value }) => {
      if (value.organizationId.trim().length === 0) {
        showError(t('resources.iam.users.messages.organizationRequired'))
        return
      }

      setIsSubmitting(true)

      try {
        const created = await createUser(value)
        showSuccess(t('resources.iam.users.notifications.created'))
        navigate(APP_ROUTES.userView(created.id))
      } catch (error) {
        notifyError(error, t('shared.errors.generic'))
      } finally {
        setIsSubmitting(false)
      }
    },
  })

  return (
    <main className="page">
      <div className="page-header">
        <h1 className="page-title">{t('resources.iam.users.pages.create')}</h1>
        <Button onClick={() => navigate(APP_ROUTES.users)} type="button" variant="outline">
          <ArrowLeft data-icon="inline-start" />
          {t('shared.actions.back')}
        </Button>
      </div>
      <Card>
        <CardContent>
          <form className="edit-form" onSubmit={(event) => {
            event.preventDefault()
            void form.handleSubmit()
          }}>
            <FieldGroup>
              <form.Field name="organizationId">
                {(field) => (
                  <Field>
                    <FieldLabel>{t('resources.iam.users.fields.organizationId')}</FieldLabel>
                    <OrganizationSelect
                      onValueChange={field.handleChange}
                      value={field.state.value}
                    />
                  </Field>
                )}
              </form.Field>
              <form.Field name="name">
                {(field) => (
                  <Field>
                    <FieldLabel htmlFor={field.name}>{t('resources.iam.users.fields.name')}</FieldLabel>
                    <Input id={field.name} onBlur={field.handleBlur} onChange={(event) => field.handleChange(event.currentTarget.value)} required value={field.state.value} />
                  </Field>
                )}
              </form.Field>
              <form.Field name="email">
                {(field) => (
                  <Field>
                    <FieldLabel htmlFor={field.name}>{t('resources.iam.users.fields.email')}</FieldLabel>
                    <Input id={field.name} onBlur={field.handleBlur} onChange={(event) => field.handleChange(event.currentTarget.value)} required type="email" value={field.state.value} />
                  </Field>
                )}
              </form.Field>
              <form.Field name="password">
                {(field) => (
                  <Field>
                    <FieldLabel htmlFor={field.name}>{t('resources.iam.users.fields.password')}</FieldLabel>
                    <Input id={field.name} onBlur={field.handleBlur} onChange={(event) => field.handleChange(event.currentTarget.value)} required type="password" value={field.state.value} />
                  </Field>
                )}
              </form.Field>
              <form.Field name="language">
                {(field) => (
                  <Field>
                    <FieldLabel>{t('resources.iam.users.fields.language')}</FieldLabel>
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
              <Button disabled={isSubmitting} type="submit">{t('resources.iam.users.actions.create')}</Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </main>
  )
}
