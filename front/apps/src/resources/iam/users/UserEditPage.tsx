import { zodResolver } from '@hookform/resolvers/zod'
import { ArrowLeft } from 'lucide-react'
import { useCallback, useEffect, useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { useNavigate, useParams } from 'react-router-dom'
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
import { LANGUAGE_CODES, LANGUAGE_OPTIONS } from '../../../shared/languages'
import { OrganizationSelect } from '../organizations/OrganizationSelect'
import { UserAccessTabs } from './UserAccessTabs'
import { createUser, getUser, updateUser } from './userApi'
import { USER_REQUEST_FIELDS, type UserDto } from './userTypes'
import { toTranslatedOptions } from './userUi'

type UserEditForm = {
  email: string
  isActive: boolean
  language: string
  name: string
  organizationId: string
  password: string
}

function getUserEditSchema(isCreate: boolean) {
  return z.object({
    email: z.string().trim().email(),
    isActive: z.boolean(),
    language: z.string().trim().min(1),
    name: z.string().trim().min(1),
    organizationId: z.string().trim().min(1),
    password: isCreate ? z.string().trim().min(1) : z.string(),
  })
}

function getEmptyUserEditForm(organizationId: string): UserEditForm {
  return {
    email: '',
    isActive: true,
    language: LANGUAGE_CODES.english,
    name: '',
    organizationId,
    password: '',
  }
}

function toForm(user: UserDto): UserEditForm {
  return {
    email: user.email,
    isActive: user.isActive,
    language: user.language,
    name: user.name,
    organizationId: user.organizationId,
    password: '',
  }
}

export function UserEditPage() {
  const t = useTranslate()
  const navigate = useNavigate()
  const notifyError = useNotifyError()
  const { user: loggedUser } = useAuth()
  const { id } = useParams()
  const [user, setUser] = useState<UserDto | null>(null)
  const isCreate = id === undefined
  const loggedOrganizationId = loggedUser?.organizationId ?? ''

  const loadUser = useCallback(async () => {
    if (isCreate) {
      return
    }

    try {
      const loaded = await getUser(id)
      setUser(loaded)
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    }
  }, [id, isCreate, notifyError, t])

  useEffect(() => {
    void loadUser()
  }, [loadUser])

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
            <UserEditFormPanel
              key={isCreate ? `create-${loggedOrganizationId}` : user?.id}
              isCreate={isCreate}
              loggedOrganizationId={loggedOrganizationId}
              onReload={loadUser}
              user={user}
            />
          )}
        </CardContent>
      </Card>
    </main>
  )
}

type UserEditFormPanelProps = {
  isCreate: boolean
  loggedOrganizationId: string
  onReload: () => Promise<void>
  user: UserDto | null
}

function UserEditFormPanel({
  isCreate,
  loggedOrganizationId,
  onReload,
  user,
}: UserEditFormPanelProps) {
  const t = useTranslate()
  const navigate = useNavigate()
  const notifyError = useNotifyError()
  const { showError, showSuccess } = useToast()
  const [isSaving, setIsSaving] = useState(false)
  const defaultValues = isCreate || user === null
    ? getEmptyUserEditForm(loggedOrganizationId)
    : toForm(user)
  const {
    control,
    handleSubmit,
    register,
  } = useForm<UserEditForm>({
    defaultValues,
    resolver: zodResolver(getUserEditSchema(isCreate)),
  })

  async function handleSave(value: UserEditForm) {
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
        if (user === null) {
          return
        }

        await updateUser(user.id, {
          [USER_REQUEST_FIELDS.name]: value.name,
          [USER_REQUEST_FIELDS.isActive]: value.isActive,
          [USER_REQUEST_FIELDS.language]: value.language,
        })
        showSuccess(t('features.iam.users.notifications.updated'))
        await onReload()
      }
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    } finally {
      setIsSaving(false)
    }
  }

  return (
    <div className="detail-stack">
      <form className="edit-form" onSubmit={handleSubmit(handleSave)}>
        <FieldGroup>
          <Controller
            control={control}
            name="organizationId"
            render={({ field }) => (
              <Field data-disabled>
                <FieldLabel>{t('shared.fields.organizationId')}</FieldLabel>
                <OrganizationSelect
                  disabled
                  includeInactive
                  onValueChange={field.onChange}
                  value={field.value}
                />
              </Field>
            )}
          />
          <Field>
            <FieldLabel htmlFor="name">{t('shared.fields.name')}</FieldLabel>
            <Input id="name" required {...register('name')} />
          </Field>
          <Field data-disabled={!isCreate}>
            <FieldLabel htmlFor="email">{t('shared.fields.email')}</FieldLabel>
            <Input disabled={!isCreate} id="email" required type="email" {...register('email')} />
          </Field>
          {isCreate && (
            <Field>
              <FieldLabel htmlFor="password">{t('shared.fields.password')}</FieldLabel>
              <Input id="password" required type="password" {...register('password')} />
            </Field>
          )}
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
          <Controller
            control={control}
            name="isActive"
            render={({ field }) => (
              <Checkbox
                checked={field.value}
                label={t('shared.fields.isActive')}
                onCheckedChange={field.onChange}
              />
            )}
          />
        </FieldGroup>
        <div className="form-actions">
          <Button disabled={isSaving} type="submit">
            {isCreate ? t('shared.actions.create') : t('shared.actions.save')}
          </Button>
        </div>
      </form>
      {!isCreate && user !== null && <UserAccessTabs userId={user.id} />}
    </div>
  )
}
