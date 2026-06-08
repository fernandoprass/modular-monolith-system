import { Button, Checkbox, Group, Paper, Select, Stack, TextInput } from '@mantine/core'
import { notifications } from '@mantine/notifications'
import { IconEdit, IconArrowLeft } from '@tabler/icons-react'
import { useCallback, useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { useNavigate, useParams } from 'react-router-dom'

import { APP_ROUTES } from '../../../app/routes'
import { useTranslate } from '../../../app/i18n/i18n'
import { notifyError } from '../../../auth/AuthProvider'
import { LANGUAGE_CODES, LANGUAGE_OPTIONS } from '../../../shared/languages'
import { OrganizationCodeEditModal } from './OrganizationCodeEditModal'
import { getOrganization, updateOrganization } from './organizationApi'
import { ORGANIZATION_REQUEST_FIELDS, type OrganizationDto } from './organizationTypes'
import { toTranslatedOptions } from './organizationUi'

type OrganizationEditForm = {
  defaultLanguage: string
  description: string
  isActive: boolean
  name: string
}

export function OrganizationEditPage() {
  const t = useTranslate()
  const navigate = useNavigate()
  const { id } = useParams()
  const [organization, setOrganization] = useState<OrganizationDto | null>(null)
  const [form, setForm] = useState<OrganizationEditForm>({
    defaultLanguage: LANGUAGE_CODES.english,
    description: '',
    isActive: true,
    name: '',
  })
  const [isSaving, setIsSaving] = useState(false)
  const [isCodeModalOpen, setIsCodeModalOpen] = useState(false)

  const loadOrganization = useCallback(async () => {
    if (id === undefined) {
      return
    }

    try {
      const loaded = await getOrganization(id)
      setOrganization(loaded)
      setForm({
        defaultLanguage: loaded.defaultLanguage,
        description: loaded.description ?? '',
        isActive: loaded.isActive,
        name: loaded.name,
      })
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    }
  }, [id, t])

  useEffect(() => {
    void loadOrganization()
  }, [loadOrganization])

  function setField<TField extends keyof OrganizationEditForm>(
    field: TField,
    value: OrganizationEditForm[TField],
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
      await updateOrganization(id, {
        [ORGANIZATION_REQUEST_FIELDS.name]: form.name,
        [ORGANIZATION_REQUEST_FIELDS.description]: form.description,
        [ORGANIZATION_REQUEST_FIELDS.isActive]: form.isActive,
        [ORGANIZATION_REQUEST_FIELDS.defaultLanguage]: form.defaultLanguage,
      })
      notifications.show({
        color: 'green',
        message: t('resources.iam.organizations.notifications.updated'),
      })
      await loadOrganization()
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    } finally {
      setIsSaving(false)
    }
  }

  return (
    <main className="page">
      <Group justify="space-between" mb="sm">
        <h1 className="page-title">{t('resources.iam.organizations.pages.edit')}</h1>
        <Button
          leftSection={<IconArrowLeft size={16} />}
          variant="default"
          onClick={() => navigate(APP_ROUTES.organizations)}
        >
          {t('shared.actions.close')}
        </Button>
      </Group>
      <Paper p="md" withBorder>
        {organization === null ? (
          <p className="page-subtitle">{t('shared.common.loading')}</p>
        ) : (
          <form onSubmit={handleSubmit}>
            <Stack gap="sm" maw={620}>
              <Group align="end" wrap="nowrap">
                <TextInput
                  disabled
                  label={t('resources.iam.organizations.fields.code')}
                  value={organization.code}
                  className="grow-field"
                />
                <Button
                  leftSection={<IconEdit size={16} />}
                  onClick={() => setIsCodeModalOpen(true)}
                  variant="default"
                >
                  {t('resources.iam.organizations.actions.editCode')}
                </Button>
              </Group>
              <TextInput
                label={t('resources.iam.organizations.fields.name')}
                onChange={(event) => setField('name', event.currentTarget.value)}
                required
                value={form.name}
              />
              <TextInput
                label={t('resources.iam.organizations.fields.description')}
                onChange={(event) => setField('description', event.currentTarget.value)}
                required
                value={form.description}
              />
              <Select
                data={toTranslatedOptions(LANGUAGE_OPTIONS, t)}
                label={t('resources.iam.organizations.fields.defaultLanguage')}
                onChange={(value) => setField('defaultLanguage', value ?? LANGUAGE_CODES.english)}
                required
                value={form.defaultLanguage}
              />
              <Checkbox
                checked={form.isActive}
                label={t('resources.iam.organizations.fields.isActive')}
                onChange={(event) => setField('isActive', event.currentTarget.checked)}
              />
              <Group justify="flex-end">
                <Button loading={isSaving} type="submit">
                  {t('shared.actions.save')}
                </Button>
              </Group>
            </Stack>
          </form>
        )}
      </Paper>
      {organization !== null && (
        <OrganizationCodeEditModal
          isOpen={isCodeModalOpen}
          onClose={() => setIsCodeModalOpen(false)}
          onSaved={loadOrganization}
          organization={organization}
        />
      )}
    </main>
  )
}
