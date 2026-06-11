import { useForm } from '@tanstack/react-form'
import { ArrowLeft } from 'lucide-react'
import { useCallback, useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useToast } from '../../../app/ToastProvider'
import { useTranslate } from '../../../app/i18n/i18n'
import { APP_ROUTES } from '../../../app/routes'
import { useNotifyError } from '../../../auth/AuthProvider'
import { Button } from '../../../components/ui/button'
import { Card, CardContent } from '../../../components/ui/card'
import { Checkbox } from '../../../components/ui/checkbox'
import { Field, FieldGroup, FieldLabel } from '../../../components/ui/form'
import { Input } from '../../../components/ui/input'
import { Select } from '../../../components/ui/select'
import { LANGUAGE_CODES, LANGUAGE_OPTIONS } from '../../../shared/languages'
import { OrganizationSelect } from '../organizations/OrganizationSelect'
import { UserAccessTabs } from './UserAccessTabs'
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
  const [isSaving, setIsSaving] = useState(false)
  const form = useForm({
    defaultValues: {
      isActive: true,
      language: LANGUAGE_CODES.english,
      name: '',
    } as UserEditForm,
    onSubmit: async ({ value }) => {
      if (id === undefined) {
        return
      }

      setIsSaving(true)

      try {
        await updateUser(id, {
          [USER_REQUEST_FIELDS.name]: value.name,
          [USER_REQUEST_FIELDS.isActive]: value.isActive,
          [USER_REQUEST_FIELDS.language]: value.language,
        })
        showSuccess(t('resources.iam.users.notifications.updated'))
        await loadUser()
      } catch (error) {
        notifyError(error, t('shared.errors.generic'))
      } finally {
        setIsSaving(false)
      }
    },
  })

  const loadUser = useCallback(async () => {
    if (id === undefined) {
      return
    }

    try {
      const loaded = await getUser(id)
      setUser(loaded)
      form.reset({
        isActive: loaded.isActive,
        language: loaded.language,
        name: loaded.name,
      })
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    }
  }, [form, id, notifyError, t])

  useEffect(() => {
    void loadUser()
  }, [loadUser])

  return (
    <main className="page">
      <div className="page-header">
        <h1 className="page-title">{t('resources.iam.users.pages.edit')}</h1>
        <Button onClick={() => navigate(APP_ROUTES.users)} type="button" variant="outline">
          <ArrowLeft data-icon="inline-start" />
          {t('shared.actions.back')}
        </Button>
      </div>
      <Card>
        <CardContent>
          {user === null ? (
            <p className="page-subtitle">{t('shared.common.loading')}</p>
          ) : (
            <div className="detail-stack">
                <form className="edit-form" onSubmit={(event) => {
                  event.preventDefault()
                  void form.handleSubmit()
                }}>
                  <FieldGroup>
                    <Field data-disabled>
                      <FieldLabel>{t('resources.iam.users.fields.organizationId')}</FieldLabel>
                      <OrganizationSelect
                        disabled
                        includeInactive
                        onValueChange={() => undefined}
                        value={user.organizationId}
                      />
                    </Field>
                    <Field data-disabled>
                      <FieldLabel>{t('resources.iam.users.fields.email')}</FieldLabel>
                      <Input disabled value={user.email} />
                    </Field>
                    <form.Field name="name">
                      {(field) => (
                        <Field>
                          <FieldLabel htmlFor={field.name}>{t('resources.iam.users.fields.name')}</FieldLabel>
                          <Input id={field.name} onBlur={field.handleBlur} onChange={(event) => field.handleChange(event.currentTarget.value)} required value={field.state.value} />
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
                    <form.Field name="isActive">
                      {(field) => (
                        <Checkbox
                          checked={field.state.value}
                          label={t('resources.iam.users.fields.isActive')}
                          onCheckedChange={(checked) => field.handleChange(checked === true)}
                        />
                      )}
                    </form.Field>
                  </FieldGroup>
                  <div className="form-actions">
                    <Button disabled={isSaving} type="submit">{t('shared.actions.save')}</Button>
                  </div>
                </form>
              <UserAccessTabs userId={user.id} />
            </div>
          )}
        </CardContent>
      </Card>
    </main>
  )
}
