import { toTranslatedOptions } from '../../../shared/translatedOptions'
import { tokenStorage } from '../../../auth/tokenStorage'
import { zodResolver } from '@hookform/resolvers/zod'
import { createValidationErrorMap } from '../../../shared/validation'
import { useCallback, useEffect, useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { z } from 'zod'

import { useToast } from '../../../app/ToastProvider'
import { useTranslate } from '../../../app/i18n/i18n'
import { useAuth, useNotifyError } from '../../../auth/AuthProvider'
import { Button } from '../../../components/ui/button'
import { Card, CardContent } from '../../../components/ui/card'
import { Field, FieldError, FieldGroup, FieldLabel } from '../../../components/ui/form'
import { Input } from '../../../components/ui/input'
import { Select } from '../../../components/ui/select'
import { LANGUAGE_OPTIONS } from '../../../shared/languages'
import { getCurrentUser, updateCurrentUser } from './userApi'
import { UserAccessTabs } from './UserAccessTabs'
import { USER_REQUEST_FIELDS, type UserDto } from './userTypes'

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

export function UserProfileSection() {
  const t = useTranslate()
  const notifyError = useNotifyError()
  const [user, setUser] = useState<UserDto | null>(null)

  const loadUser = useCallback(async () => {
    try {
      const loaded = await getCurrentUser()
      setUser(loaded)
    } catch (error) {
      notifyError(error, t('common.messages.errors.generic'))
    }
  }, [notifyError, t])

  useEffect(() => {
    void loadUser()
  }, [loadUser])

  return (
    <>
      <Card>
        <CardContent>
          {user === null ? (
            <p className="page-subtitle">{t('common.states.loading')}</p>
          ) : (
            <UserProfileFormPanel key={user.id} onSaved={loadUser} user={user} />
          )}
        </CardContent>
      </Card>
    </>
  )
}

export function UserSecuritySection() {
  const t = useTranslate()
  const { user } = useAuth()

  return user === null
    ? <p className="page-subtitle">{t('common.states.loading')}</p>
    : <UserAccessTabs userId={user.id} />
}

type UserProfileFormPanelProps = {
  onSaved: () => Promise<void>
  user: UserDto
}

function UserProfileFormPanel({ onSaved, user }: UserProfileFormPanelProps) {
  const t = useTranslate()
  const { updateProfile } = useAuth()
  const notifyError = useNotifyError()
  const { showSuccess } = useToast()
  const [isSaving, setIsSaving] = useState(false)
  const {
    formState: { errors },
    control,
    handleSubmit,
    register,
  } = useForm<UserProfileForm>({
    defaultValues: toForm(user),
    resolver: zodResolver(userProfileSchema, { error: createValidationErrorMap(t) }),
  })

  async function handleSave(value: UserProfileForm) {
    const session = tokenStorage.getSession()
    setIsSaving(true)

    try {
      const message = await updateCurrentUser({
        [USER_REQUEST_FIELDS.name]: value.name,
        [USER_REQUEST_FIELDS.isActive]: user.isActive,
        [USER_REQUEST_FIELDS.language]: value.language,
      })
      if (!updateProfile(user.id, { fullName: value.name, language: value.language }, session)) return
      showSuccess(message)
      await onSaved()
    } catch (error) {
      if (tokenStorage.isCurrentSession(session)) notifyError(error, t('common.messages.errors.generic'))
    } finally {
      setIsSaving(false)
    }
  }

  return (
    <form noValidate className="edit-form" onSubmit={handleSubmit(handleSave)}>
      <FieldGroup>
        <Field data-disabled>
          <FieldLabel>{t('common.labels.email')}</FieldLabel>
          <Input disabled value={user.email} />
        </Field>
        <Field data-invalid={errors.name !== undefined}>
          <FieldLabel htmlFor="name">{t('common.labels.name')}</FieldLabel>
          <Input
            aria-invalid={errors.name !== undefined}
            aria-describedby={errors.name ? 'UserProfilePage-name-error' : undefined}
            id="name"
            required
            {...register('name')}
          />
          <FieldError id="UserProfilePage-name-error">{errors.name?.message}</FieldError>
        </Field>
        <Controller
          control={control}
          name="language"
          render={({ field }) => (
            <Field data-invalid={errors.language !== undefined}>
              <FieldLabel htmlFor="UserProfilePage-language">{t('common.labels.language')}</FieldLabel>
              <Select
                onBlur={field.onBlur}
                ref={field.ref}
                aria-invalid={errors.language !== undefined}
                aria-describedby={errors.language ? 'UserProfilePage-language-error' : undefined}
                id="UserProfilePage-language"
                onValueChange={field.onChange}
                options={toTranslatedOptions(LANGUAGE_OPTIONS, t)}
                value={field.value}
              />
              <FieldError id="UserProfilePage-language-error">{errors.language?.message}</FieldError>
            </Field>
          )}
        />
      </FieldGroup>
      <div className="form-actions">
        <Button disabled={isSaving} type="submit">{t('common.actions.save')}</Button>
      </div>
    </form>
  )
}
