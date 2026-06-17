import { useForm } from '@tanstack/react-form'
import { ArrowLeft } from 'lucide-react'
import { useCallback, useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
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
import { LANGUAGE_CODES } from '../../../shared/languages'
import { OrganizationSelect } from '../organizations/OrganizationSelect'
import { UserAccessTabs } from './UserAccessTabs'
import { createUser, getUser, updateUser } from './userApi'
import { USER_REQUEST_FIELDS, type UserDto } from './userTypes'

type UserEditForm = {
  email: string
  isActive: boolean
  language: string
  name: string
  organizationId: string
  password: string
}

export function UserEditPage() {
  const t = useTranslate()
  const navigate = useNavigate()
  const notifyError = useNotifyError()
  const { user: loggedUser } = useAuth()
  const { showError, showSuccess } = useToast()
  const { id } = useParams()
  const [user, setUser] = useState<UserDto | null>(null)
  const [isSaving, setIsSaving] = useState(false)
  const isCreate = id === undefined
  const form = useForm({
    defaultValues: {
      email: '',
      isActive: true,
      language: LANGUAGE_CODES.english,
      name: '',
      organizationId: loggedUser?.organizationId ?? '',
      password: '',
    } as UserEditForm,
    onSubmit: async ({ value }) => {
      if (value.organizationId.trim().length === 0) {
        showError(t('features.iam.users.messages.organizationRequired'))
        return
      }

      setIsSaving(true)

      try {
        if (isCreate) {
          const created = await createUser(value)
          showSuccess(t('features.iam.users.notifications.created'))
          navigate(APP_ROUTES.userView(created.id))
        } else {
          await updateUser(id, {
            [USER_REQUEST_FIELDS.name]: value.name,
            [USER_REQUEST_FIELDS.isActive]: value.isActive,
            [USER_REQUEST_FIELDS.language]: value.language,
          })
          showSuccess(t('features.iam.users.notifications.updated'))
          await loadUser()
        }
      } catch (error) {
        notifyError(error, t('shared.errors.generic'))
      } finally {
        setIsSaving(false)
      }
    },
  })

  const loadUser = useCallback(async () => {
    if (isCreate) {
      return
    }

    try {
      const loaded = await getUser(id)
      setUser(loaded)
      form.setFieldValue('email', loaded.email)
      form.setFieldValue('isActive', loaded.isActive)
      form.setFieldValue('language', loaded.language)
      form.setFieldValue('name', loaded.name)
      form.setFieldValue('organizationId', loaded.organizationId)
      form.setFieldValue('password', '')
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    }
  }, [form, id, isCreate, notifyError, t])

  useEffect(() => {
    void loadUser()
  }, [loadUser])

  useEffect(() => {
    if (isCreate) {
      form.reset({
        email: '',
        isActive: true,
        language: LANGUAGE_CODES.english,
        name: '',
        organizationId: loggedUser?.organizationId ?? '',
        password: '',
      })
      return
    }

  }, [form, isCreate, loggedUser?.organizationId])

  return (
    <main className="page">
      <div className="page-header">
        <h1 className="page-title">{isCreate ? t('features.iam.users.pages.create') : t('features.iam.users.pages.edit')}</h1>
        <Button onClick={() => navigate(APP_ROUTES.users)} type="button" variant="outline">
          <ArrowLeft data-icon="inline-start" />
          {t('shared.actions.back')}
        </Button>
      </div>
      <Card>
        <CardContent>
          {!isCreate && user === null ? (
            <p className="page-subtitle">{t('shared.common.loading')}</p>
          ) : (
            <div className="detail-stack">
                <form className="edit-form" onSubmit={(event) => {
                  event.preventDefault()
                  void form.handleSubmit()
                }}>
                  <FieldGroup>
                    <form.Field name="organizationId">
                      {(field) => (
                        <Field data-disabled>
                          <FieldLabel>{t('shared.fields.organizationId')}</FieldLabel>
                          <OrganizationSelect
                            disabled
                            includeInactive
                            onValueChange={field.handleChange}
                            value={field.state.value}
                          />
                        </Field>
                      )}
                    </form.Field>
                    <form.Field name="name">
                      {(field) => (
                        <Field>
                          <FieldLabel htmlFor={field.name}>{t('shared.fields.name')}</FieldLabel>
                          <Input id={field.name} onBlur={field.handleBlur} onChange={(event) => field.handleChange(event.currentTarget.value)} required value={field.state.value} />
                        </Field>
                      )}
                    </form.Field>
                    <form.Field name="email">
                      {(field) => (
                        <Field data-disabled={!isCreate}>
                          <FieldLabel htmlFor={field.name}>{t('shared.fields.email')}</FieldLabel>
                          <Input disabled={!isCreate} id={field.name} onBlur={field.handleBlur} onChange={(event) => field.handleChange(event.currentTarget.value)} required type="email" value={field.state.value} />
                        </Field>
                      )}
                    </form.Field>
                    {isCreate && (
                      <form.Field name="password">
                        {(field) => (
                          <Field>
                            <FieldLabel htmlFor={field.name}>{t('shared.fields.password')}</FieldLabel>
                            <Input id={field.name} onBlur={field.handleBlur} onChange={(event) => field.handleChange(event.currentTarget.value)} required type="password" value={field.state.value} />
                          </Field>
                        )}
                      </form.Field>
                    )}
                    <form.Field name="language">
                      {(field) => (
                        <Field>
                          <FieldLabel>{t('shared.fields.language')}</FieldLabel>
                          <Select
                            onValueChange={field.handleChange}
                            options={[
                              { label: t('shared.languages.en'), value: LANGUAGE_CODES.english },
                              { label: t('shared.languages.ptbr'), value: LANGUAGE_CODES.portugueseBrazil },
                              { label: t('shared.languages.es'), value: LANGUAGE_CODES.spanish },
                            ]}
                            value={field.state.value}
                          />
                        </Field>
                      )}
                    </form.Field>
                    <form.Field name="isActive">
                      {(field) => (
                        <Checkbox
                          checked={field.state.value}
                          label={t('shared.fields.isActive')}
                          onCheckedChange={(checked) => field.handleChange(checked === true)}
                        />
                      )}
                    </form.Field>
                  </FieldGroup>
                  <div className="form-actions">
                    <Button disabled={isSaving} type="submit">
                      {isCreate ? t('shared.actions.create') : t('shared.actions.save')}
                    </Button>
                  </div>
                </form>
              {!isCreate && user !== null && <UserAccessTabs userId={user.id} />}
            </div>
          )}
        </CardContent>
      </Card>
    </main>
  )
}
