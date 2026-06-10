import { useCallback, useEffect, useState } from 'react'
import type { FormEvent } from 'react'

import { useToast } from '../../../app/ToastProvider'
import { useTranslate } from '../../../app/i18n/i18n'
import { useNotifyError } from '../../../auth/AuthProvider'
import { Button } from '../../../components/ui/button'
import { Card, CardContent } from '../../../components/ui/card'
import { Field } from '../../../components/ui/field'
import { Input } from '../../../components/ui/input'
import { Select } from '../../../components/ui/select'
import { LANGUAGE_CODES, LANGUAGE_OPTIONS } from '../../../shared/languages'
import { OrganizationSelect } from '../organizations/OrganizationSelect'
import { getCurrentUser, updateCurrentUser } from './userApi'
import { USER_REQUEST_FIELDS, type UserDto } from './userTypes'
import { toTranslatedOptions } from './userUi'

type UserProfileForm = {
  language: string
  name: string
}

export function UserProfilePage() {
  const t = useTranslate()
  const notifyError = useNotifyError()
  const { showSuccess } = useToast()
  const [user, setUser] = useState<UserDto | null>(null)
  const [form, setForm] = useState<UserProfileForm>({
    language: LANGUAGE_CODES.english,
    name: '',
  })
  const [isSaving, setIsSaving] = useState(false)

  const loadUser = useCallback(async () => {
    try {
      const loaded = await getCurrentUser()
      setUser(loaded)
      setForm({
        language: loaded.language,
        name: loaded.name,
      })
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    }
  }, [notifyError, t])

  useEffect(() => {
    void loadUser()
  }, [loadUser])

  function setField<TField extends keyof UserProfileForm>(
    field: TField,
    value: UserProfileForm[TField],
  ) {
    setForm((currentForm) => ({
      ...currentForm,
      [field]: value,
    }))
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    if (user === null) {
      return
    }

    setIsSaving(true)

    try {
      await updateCurrentUser({
        [USER_REQUEST_FIELDS.name]: form.name,
        [USER_REQUEST_FIELDS.isActive]: user.isActive,
        [USER_REQUEST_FIELDS.language]: form.language,
      })
      showSuccess(t('resources.iam.users.notifications.profileUpdated'))
      await loadUser()
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    } finally {
      setIsSaving(false)
    }
  }

  return (
    <main className="page">
      <h1 className="page-title">{t('resources.iam.users.pages.profile')}</h1>
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
