import { ArrowLeft } from 'lucide-react'
import { useState } from 'react'
import type { FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'

import { useToast } from '../../../app/ToastProvider'
import { useTranslate } from '../../../app/i18n/i18n'
import { APP_ROUTES } from '../../../app/routes'
import { useNotifyError } from '../../../auth/AuthProvider'
import { Button } from '../../../components/ui/button'
import { Card, CardContent } from '../../../components/ui/card'
import { Field } from '../../../components/ui/field'
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
  const { showSuccess } = useToast()
  const [form, setForm] = useState<UserCreateForm>({
    email: '',
    language: LANGUAGE_CODES.english,
    name: '',
    organizationId: '',
    password: '',
  })
  const [isSubmitting, setIsSubmitting] = useState(false)

  function setField<TField extends keyof UserCreateForm>(
    field: TField,
    value: UserCreateForm[TField],
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
      const created = await createUser(form)
      showSuccess(t('resources.iam.users.notifications.created'))
      navigate(APP_ROUTES.userShow(created.id))
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <main className="page">
      <div className="page-header">
        <h1 className="page-title">{t('resources.iam.users.pages.create')}</h1>
        <Button onClick={() => navigate(APP_ROUTES.users)} type="button" variant="outline">
          <ArrowLeft size={16} />
          {t('shared.actions.back')}
        </Button>
      </div>
      <Card>
        <CardContent>
          <form className="edit-form" onSubmit={handleSubmit}>
            <Field label={t('resources.iam.users.fields.organizationId')}>
              <OrganizationSelect
                onValueChange={(value) => setField('organizationId', value)}
                value={form.organizationId}
              />
            </Field>
            <Field label={t('resources.iam.users.fields.name')}>
              <Input onChange={(event) => setField('name', event.currentTarget.value)} required value={form.name} />
            </Field>
            <Field label={t('resources.iam.users.fields.email')}>
              <Input onChange={(event) => setField('email', event.currentTarget.value)} required type="email" value={form.email} />
            </Field>
            <Field label={t('resources.iam.users.fields.password')}>
              <Input onChange={(event) => setField('password', event.currentTarget.value)} required type="password" value={form.password} />
            </Field>
            <Field label={t('resources.iam.users.fields.language')}>
              <Select
                onValueChange={(value) => setField('language', value)}
                options={toTranslatedOptions(LANGUAGE_OPTIONS, t)}
                value={form.language}
              />
            </Field>
            <div className="form-actions">
              <Button disabled={isSubmitting} type="submit">{t('resources.iam.users.actions.create')}</Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </main>
  )
}
