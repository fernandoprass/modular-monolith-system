import { toTranslatedOptions } from '../../../shared/translatedOptions'
import { zodResolver } from '@hookform/resolvers/zod'
import { createValidationErrorMap } from '../../../shared/validation'
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
import { Field, FieldError, FieldGroup, FieldLabel } from '../../../components/ui/form'
import { Input } from '../../../components/ui/input'
import { Select } from '../../../components/ui/select'
import { LANGUAGE_CODES, LANGUAGE_OPTIONS } from '../../../shared/languages'
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
      notifyError(error, t('common.messages.errors.generic'))
    }
  }, [id, isCreate, notifyError, t])

  useEffect(() => {
    void loadUser()
  }, [loadUser])

  return (
    <main className="page">
      <div className="page-header">
        <h1 className="page-title">{isCreate ? t('common.pages.create', { entity: t('common.labels.user') }) : t('common.pages.edit', { entity: t('common.labels.user') })}</h1>
        <Button onClick={() => navigate(APP_ROUTES.users)} type="button" variant="outline">
          <ArrowLeft data-icon="inline-start" />
          {t('common.actions.back')}
        </Button>
      </div>
      <Card>
        <CardContent>
          {!isCreate && user === null ? (
            <p className="page-subtitle">{t('common.states.loading')}</p>
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
    formState: { errors },
    control,
    handleSubmit,
    register,
  } = useForm<UserEditForm>({
    defaultValues,
    resolver: zodResolver(getUserEditSchema(isCreate), { error: createValidationErrorMap(t) }),
  })

  async function handleSave(value: UserEditForm) {
    if (value.organizationId.trim().length === 0) {
      showError(t('features.iam.messages.users.organizationRequired'))
      return
    }

    setIsSaving(true)

    try {
      if (isCreate) {
        const { data: created, message } = await createUser(value)
        showSuccess(message)
        navigate(APP_ROUTES.userView(created.id))
      } else {
        if (user === null) {
          return
        }

        const message = await updateUser(user.id, {
          [USER_REQUEST_FIELDS.name]: value.name,
          [USER_REQUEST_FIELDS.isActive]: value.isActive,
          [USER_REQUEST_FIELDS.language]: value.language,
        })
        showSuccess(message)
        await onReload()
      }
    } catch (error) {
      notifyError(error, t('common.messages.errors.generic'))
    } finally {
      setIsSaving(false)
    }
  }

  return (
    <div className="detail-stack">
      <form noValidate className="edit-form" onSubmit={handleSubmit(handleSave)}>
        <FieldGroup>
          <Controller
            control={control}
            name="organizationId"
            render={({ field }) => (
              <Field data-disabled data-invalid={errors.organizationId !== undefined}>
                <FieldLabel htmlFor="UserEditPage-organizationId">{t('features.iam.labels.organizationId')}</FieldLabel>
                <OrganizationSelect
                  onBlur={field.onBlur}
                  ref={field.ref}
                  aria-invalid={errors.organizationId !== undefined}
                  aria-describedby={errors.organizationId ? 'UserEditPage-organizationId-error' : undefined}
                  id="UserEditPage-organizationId"
                  disabled
                  includeInactive
                  onValueChange={field.onChange}
                  value={field.value}
                />
                <FieldError id="UserEditPage-organizationId-error">{errors.organizationId?.message}</FieldError>
              </Field>
            )}
          />
          <Field data-invalid={errors.name !== undefined}>
            <FieldLabel htmlFor="name">{t('common.labels.name')}</FieldLabel>
            <Input
              aria-invalid={errors.name !== undefined}
              aria-describedby={errors.name ? 'UserEditPage-name-error' : undefined}
              id="name"
              required
              {...register('name')}
            />
            <FieldError id="UserEditPage-name-error">{errors.name?.message}</FieldError>
          </Field>
          <Field data-disabled={!isCreate} data-invalid={errors.email !== undefined}>
            <FieldLabel htmlFor="email">{t('common.labels.email')}</FieldLabel>
            <Input
              aria-invalid={errors.email !== undefined}
              aria-describedby={errors.email ? 'UserEditPage-email-error' : undefined}
              disabled={!isCreate}
              id="email"
              required
              type="email"
              {...register('email')}
            />
            <FieldError id="UserEditPage-email-error">{errors.email?.message}</FieldError>
          </Field>
          {isCreate && (
            <Field data-invalid={errors.password !== undefined}>
              <FieldLabel htmlFor="password">{t('features.iam.labels.password')}</FieldLabel>
              <Input
                aria-invalid={errors.password !== undefined}
                aria-describedby={errors.password ? 'UserEditPage-password-error' : undefined}
                id="password"
                required
                type="password"
                {...register('password')}
              />
              <FieldError id="UserEditPage-password-error">{errors.password?.message}</FieldError>
            </Field>
          )}
          <Controller
            control={control}
            name="language"
            render={({ field }) => (
              <Field data-invalid={errors.language !== undefined}>
                <FieldLabel htmlFor="UserEditPage-language">{t('common.labels.language')}</FieldLabel>
                <Select
                  onBlur={field.onBlur}
                  ref={field.ref}
                  aria-invalid={errors.language !== undefined}
                  aria-describedby={errors.language ? 'UserEditPage-language-error' : undefined}
                  id="UserEditPage-language"
                  onValueChange={field.onChange}
                  options={toTranslatedOptions(LANGUAGE_OPTIONS, t)}
                  value={field.value}
                />
                <FieldError id="UserEditPage-language-error">{errors.language?.message}</FieldError>
              </Field>
            )}
          />
          <Controller
            control={control}
            name="isActive"
            render={({ field }) => (
              <Checkbox
                checked={field.value}
                label={t('common.labels.isActive')}
                onCheckedChange={field.onChange}
              />
            )}
          />
        </FieldGroup>
        <div className="form-actions">
          <Button disabled={isSaving} type="submit">
            {isCreate ? t('common.actions.create') : t('common.actions.save')}
          </Button>
        </div>
      </form>
      {!isCreate && user !== null && <UserAccessTabs userId={user.id} />}
    </div>
  )
}
