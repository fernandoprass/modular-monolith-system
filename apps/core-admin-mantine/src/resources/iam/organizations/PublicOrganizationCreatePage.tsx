import { Anchor, Button, Paper, PasswordInput, Select, Stack, TextInput } from '@mantine/core'
import { notifications } from '@mantine/notifications'
import { useState } from 'react'
import type { FormEvent } from 'react'
import { Link } from 'react-router-dom'

import { APP_CONSTANTS } from '../../../app/appConstants'
import { useTranslate } from '../../../app/i18n/i18n'
import { APP_ROUTES } from '../../../app/routes'
import { notifyError } from '../../../auth/AuthProvider'
import { LANGUAGE_CODES, LANGUAGE_OPTIONS } from '../../../shared/languages'
import {
  ORGANIZATION_TYPES,
  ORGANIZATION_TYPE_OPTIONS,
  type OrganizationCreateForm,
} from './organizationTypes'
import { createOrganization } from './organizationApi'
import { toTranslatedOptions } from './organizationUi'

export function PublicOrganizationCreatePage() {
  const t = useTranslate()
  const [form, setForm] = useState<OrganizationCreateForm>({
    code: '',
    defaultLanguage: LANGUAGE_CODES.english,
    description: '',
    name: '',
    type: ORGANIZATION_TYPES.company,
    userEmail: '',
    userName: '',
    userPassword: '',
  })
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [isSuccess, setIsSuccess] = useState(false)
  const isCompany = form.type === ORGANIZATION_TYPES.company

  function setField<TField extends keyof OrganizationCreateForm>(
    field: TField,
    value: OrganizationCreateForm[TField],
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
      await createOrganization(form)
      setIsSuccess(true)
      notifications.show({
        color: 'green',
        message: t('public.organizationRegistration.messages.success'),
      })
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <main className="auth-page">
      <Paper className="register-card" shadow="xs" p="md" withBorder>
        <Stack gap="sm">
          <div>
            <h1 className="auth-title">{APP_CONSTANTS.appName}</h1>
            <p className="auth-subtitle">{t('public.organizationRegistration.title')}</p>
          </div>
          {isSuccess ? (
            <Stack gap="sm">
              <p className="success-text">{t('public.organizationRegistration.messages.success')}</p>
              <Anchor component={Link} to={APP_ROUTES.login}>
                {t('public.organizationRegistration.actions.signIn')}
              </Anchor>
            </Stack>
          ) : (
            <form onSubmit={handleSubmit}>
              <Stack gap="sm">
                <Select
                  data={toTranslatedOptions(ORGANIZATION_TYPE_OPTIONS, t)}
                  label={t('public.organizationRegistration.fields.type')}
                  onChange={(value) => setField('type', Number(value))}
                  required
                  value={String(form.type)}
                />
                {isCompany && (
                  <>
                    <TextInput
                      label={t('public.organizationRegistration.fields.code')}
                      onChange={(event) => setField('code', event.currentTarget.value)}
                      required
                      value={form.code}
                    />
                    <TextInput
                      label={t('public.organizationRegistration.fields.name')}
                      onChange={(event) => setField('name', event.currentTarget.value)}
                      required
                      value={form.name}
                    />
                  </>
                )}
                <TextInput
                  label={t('public.organizationRegistration.fields.description')}
                  onChange={(event) => setField('description', event.currentTarget.value)}
                  required
                  value={form.description}
                />
                <Select
                  data={toTranslatedOptions(LANGUAGE_OPTIONS, t)}
                  label={t('public.organizationRegistration.fields.defaultLanguage')}
                  onChange={(value) => setField('defaultLanguage', value ?? LANGUAGE_CODES.english)}
                  required
                  value={form.defaultLanguage}
                />
                <TextInput
                  label={t('public.organizationRegistration.fields.adminName')}
                  onChange={(event) => setField('userName', event.currentTarget.value)}
                  required
                  value={form.userName}
                />
                <TextInput
                  label={t('public.organizationRegistration.fields.adminEmail')}
                  onChange={(event) => setField('userEmail', event.currentTarget.value)}
                  required
                  type="email"
                  value={form.userEmail}
                />
                <PasswordInput
                  label={t('public.organizationRegistration.fields.adminPassword')}
                  onChange={(event) => setField('userPassword', event.currentTarget.value)}
                  required
                  value={form.userPassword}
                />
                <Button loading={isSubmitting} type="submit" fullWidth>
                  {t('public.organizationRegistration.actions.submit')}
                </Button>
                <Anchor component={Link} to={APP_ROUTES.login} ta="center" size="sm">
                  {t('public.organizationRegistration.actions.signIn')}
                </Anchor>
              </Stack>
            </form>
          )}
        </Stack>
      </Paper>
    </main>
  )
}
