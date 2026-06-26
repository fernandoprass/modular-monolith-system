import { zodResolver } from '@hookform/resolvers/zod'
import { useCallback, useEffect, useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { z } from 'zod'

import { useToast } from '../../../app/ToastProvider'
import { useTranslate } from '../../../app/i18n/i18n'
import { useAuth, useNotifyError } from '../../../auth/AuthProvider'
import { Button } from '../../../components/ui/button'
import { Card, CardContent } from '../../../components/ui/card'
import { Field, FieldGroup, FieldLabel } from '../../../components/ui/form'
import { Input } from '../../../components/ui/input'
import { Select } from '../../../components/ui/select'
import { IAM_PERMISSIONS } from '../../../shared/iamConstants'
import { LANGUAGE_OPTIONS } from '../../../shared/languages'
import { hasPermissionCode } from '../../../shared/permissions'
import { OrganizationSelect } from '../organizations/OrganizationSelect'
import { getCurrentUser, updateCurrentUser } from './userApi'
import { UserAccessTabs } from './UserAccessTabs'
import { USER_REQUEST_FIELDS, type UserDto } from './userTypes'
import { toTranslatedOptions } from './userUi'

type UserProfileForm = {
  language: string
  name: string
}

const userProfileSchema = z.object({
  language: z.string().trim().min(1),
  name: z.string().trim().min(1),
})

function toForm(user: UserDto): UserProfileForm {
  return {
    language: user.language,
    name: user.name,
  }
}

export function UserProfilePage() {
  const t = useTranslate()
  const notifyError = useNotifyError()
  const { permissions } = useAuth()
  const [user, setUser] = useState<UserDto | null>(null)
  const canViewAccess = hasPermissionCode(permissions, IAM_PERMISSIONS.userProfile.viewAccess)

  const loadUser = useCallback(async () => {
    try {
      const loaded = await getCurrentUser()
      setUser(loaded)
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    }
  }, [notifyError, t])

  useEffect(() => {
    void loadUser()
  }, [loadUser])

  return (
    <main className="page">
      <div className="page-header">
        <h1 className="page-title">{t('features.iam.users.pages.profile')}</h1>
      </div>
      <Card>
        <CardContent>
          {user === null ? (
            <p className="page-subtitle">{t('shared.common.loading')}</p>
          ) : (
            <UserProfileFormPanel key={user.id} onSaved={loadUser} user={user} />
          )}
        </CardContent>
      </Card>
      {user !== null && canViewAccess && <UserAccessTabs userId={user.id} />}
    </main>
  )
}

type UserProfileFormPanelProps = {
  onSaved: () => Promise<void>
  user: UserDto
}

function UserProfileFormPanel({ onSaved, user }: UserProfileFormPanelProps) {
  const t = useTranslate()
  const notifyError = useNotifyError()
  const { showSuccess } = useToast()
  const [isSaving, setIsSaving] = useState(false)
  const {
    control,
    handleSubmit,
    register,
  } = useForm<UserProfileForm>({
    defaultValues: toForm(user),
    resolver: zodResolver(userProfileSchema),
  })

  async function handleSave(value: UserProfileForm) {
    setIsSaving(true)

    try {
      await updateCurrentUser({
        [USER_REQUEST_FIELDS.name]: value.name,
        [USER_REQUEST_FIELDS.isActive]: user.isActive,
        [USER_REQUEST_FIELDS.language]: value.language,
      })
      showSuccess(t('features.iam.users.notifications.profileUpdated'))
      await onSaved()
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    } finally {
      setIsSaving(false)
    }
  }

  return (
    <form className="edit-form" onSubmit={handleSubmit(handleSave)}>
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
        <Field>
          <FieldLabel htmlFor="name">{t('shared.fields.name')}</FieldLabel>
          <Input id="name" required {...register('name')} />
        </Field>
        <Controller
          control={control}
          name="language"
          render={({ field }) => (
            <Field>
              <FieldLabel>{t('shared.fields.language')}</FieldLabel>
              <Select
                onValueChange={field.onChange}
                options={toTranslatedOptions(LANGUAGE_OPTIONS, t)}
                value={field.value}
              />
            </Field>
          )}
        />
      </FieldGroup>
      <div className="form-actions">
        <Button disabled={isSaving} type="submit">{t('shared.actions.save')}</Button>
      </div>
    </form>
  )
}
