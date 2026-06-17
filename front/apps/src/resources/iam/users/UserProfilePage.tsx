import { useForm } from '@tanstack/react-form'
import { useCallback, useEffect, useState } from 'react'

import { useToast } from '../../../app/ToastProvider'
import { useTranslate } from '../../../app/i18n/i18n'
import { useAuth, useNotifyError } from '../../../auth/AuthProvider'
import { Button } from '../../../components/ui/button'
import { Card, CardContent } from '../../../components/ui/card'
import { Field, FieldGroup, FieldLabel } from '../../../components/ui/form'
import { Input } from '../../../components/ui/input'
import { Select } from '../../../components/ui/select'
import { IAM_PERMISSIONS } from '../../../shared/iamConstants'
import { LANGUAGE_CODES } from '../../../shared/languages'
import { hasPermissionCode } from '../../../shared/permissions'
import { OrganizationSelect } from '../organizations/OrganizationSelect'
import { getCurrentUser, updateCurrentUser } from './userApi'
import { UserAccessTabs } from './UserAccessTabs'
import { USER_REQUEST_FIELDS, type UserDto } from './userTypes'

type UserProfileForm = {
  language: string
  name: string
}

export function UserProfilePage() {
  const t = useTranslate()
  const notifyError = useNotifyError()
  const { showSuccess } = useToast()
  const { permissions } = useAuth()
  const [user, setUser] = useState<UserDto | null>(null)
  const [isSaving, setIsSaving] = useState(false)
  const canViewAccess = hasPermissionCode(permissions, IAM_PERMISSIONS.userProfile.viewAccess)
  const form = useForm({
    defaultValues: {
      language: LANGUAGE_CODES.english,
      name: '',
    } as UserProfileForm,
    onSubmit: async ({ value }) => {
      if (user === null) {
        return
      }

      setIsSaving(true)

      try {
        await updateCurrentUser({
          [USER_REQUEST_FIELDS.name]: value.name,
          [USER_REQUEST_FIELDS.isActive]: user.isActive,
          [USER_REQUEST_FIELDS.language]: value.language,
        })
        showSuccess(t('features.iam.users.notifications.profileUpdated'))
        await loadUser()
      } catch (error) {
        notifyError(error, t('shared.errors.generic'))
      } finally {
        setIsSaving(false)
      }
    },
  })

  const loadUser = useCallback(async () => {
    try {
      const loaded = await getCurrentUser()
      setUser(loaded)
      form.setFieldValue('language', loaded.language)
      form.setFieldValue('name', loaded.name)
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    }
  }, [form, notifyError, t])

  useEffect(() => {
    void loadUser()
  }, [loadUser])

  return (
    <main className="page">
      <h1 className="page-title">{t('features.iam.users.pages.profile')}</h1>
      <Card>
        <CardContent>
          {user === null ? (
            <p className="page-subtitle">{t('shared.common.loading')}</p>
          ) : (
            <form className="edit-form" onSubmit={(event) => {
              event.preventDefault()
              void form.handleSubmit()
            }}>
              <FieldGroup>
                <Field data-disabled>
                  <FieldLabel>{t('shared.fields.organizationId')}</FieldLabel>
                  <OrganizationSelect
                    disabled
                    includeInactive
                    onValueChange={() => undefined}
                    value={user.organizationId}
                  />
                </Field>
                <Field data-disabled>
                  <FieldLabel>{t('shared.fields.email')}</FieldLabel>
                  <Input disabled value={user.email} />
                </Field>
                <form.Field name="name">
                  {(field) => (
                    <Field>
                      <FieldLabel htmlFor={field.name}>{t('shared.fields.name')}</FieldLabel>
                      <Input onBlur={field.handleBlur} onChange={(event) => field.handleChange(event.currentTarget.value)} required value={field.state.value} />
                    </Field>
                  )}
                </form.Field>
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
              </FieldGroup>
              <div className="form-actions">
                <Button disabled={isSaving} type="submit">{t('shared.actions.save')}</Button>
              </div>
            </form>
          )}
        </CardContent>
      </Card>
      {user !== null && canViewAccess && <UserAccessTabs userId={user.id} />}
    </main>
  )
}
