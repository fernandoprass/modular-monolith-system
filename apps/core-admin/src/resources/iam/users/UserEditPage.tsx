import { ArrowLeft } from 'lucide-react'
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
import { Field } from '../../../components/ui/field'
import { Input } from '../../../components/ui/input'
import { Select } from '../../../components/ui/select'
import { LANGUAGE_CODES, LANGUAGE_OPTIONS } from '../../../shared/languages'
import { OrganizationSelect } from '../organizations/OrganizationSelect'
import { getUser, updateUser } from './userApi'
import { USER_REQUEST_FIELDS, type UserDto } from './userTypes'
import { toTranslatedOptions } from './userUi'

type UserEditForm = {
  isActive: boolean
  language: string
  name: string
}

export function UserEditPage() {
  const t = useTranslate()
  const navigate = useNavigate()
  const notifyError = useNotifyError()
  const { showSuccess } = useToast()
  const { id } = useParams()
  const [user, setUser] = useState<UserDto | null>(null)
  const [form, setForm] = useState<UserEditForm>({
    isActive: true,
    language: LANGUAGE_CODES.english,
    name: '',
  })
  const [isSaving, setIsSaving] = useState(false)

  const loadUser = useCallback(async () => {
    if (id === undefined) {
      return
    }

    try {
      const loaded = await getUser(id)
      setUser(loaded)
      setForm({
        isActive: loaded.isActive,
        language: loaded.language,
        name: loaded.name,
      })
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    }
  }, [id, notifyError, t])

  useEffect(() => {
    void loadUser()
  }, [loadUser])

  function setField<TField extends keyof UserEditForm>(
    field: TField,
    value: UserEditForm[TField],
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
      await updateUser(id, {
        [USER_REQUEST_FIELDS.name]: form.name,
        [USER_REQUEST_FIELDS.isActive]: form.isActive,
        [USER_REQUEST_FIELDS.language]: form.language,
      })
      showSuccess(t('resources.iam.users.notifications.updated'))
      await loadUser()
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    } finally {
      setIsSaving(false)
    }
  }

  return (
    <main className="page">
      <div className="page-header">
        <h1 className="page-title">{t('resources.iam.users.pages.edit')}</h1>
        <Button onClick={() => navigate(APP_ROUTES.users)} type="button" variant="outline">
          <ArrowLeft size={16} />
          {t('shared.actions.back')}
        </Button>
      </div>
      <Card>
        <CardContent>
          {user === null ? (
            <p className="page-subtitle">{t('shared.common.loading')}</p>
          ) : (
            <form className="edit-form" onSubmit={handleSubmit}>
              <Field label={t('resources.iam.users.fields.organizationId')}>
                <OrganizationSelect
                  disabled
                  includeInactive
                  onValueChange={() => undefined}
                  value={user.organizationId}
                />
              </Field>
              <Field label={t('resources.iam.users.fields.email')}>
                <Input disabled value={user.email} />
              </Field>
              <Field label={t('resources.iam.users.fields.name')}>
                <Input onChange={(event) => setField('name', event.currentTarget.value)} required value={form.name} />
              </Field>
              <Field label={t('resources.iam.users.fields.language')}>
                <Select
                  onValueChange={(value) => setField('language', value)}
                  options={toTranslatedOptions(LANGUAGE_OPTIONS, t)}
                  value={form.language}
                />
              </Field>
              <Checkbox
                checked={form.isActive}
                label={t('resources.iam.users.fields.isActive')}
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
