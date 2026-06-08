import { Badge, Button, Group, Paper, SimpleGrid, Stack, Text } from '@mantine/core'
import { IconArrowLeft } from '@tabler/icons-react'
import { useEffect, useState } from 'react'
import type { ReactNode } from 'react'
import { useNavigate, useParams } from 'react-router-dom'

import { APP_ROUTES } from '../../../app/routes'
import { useTranslate } from '../../../app/i18n/i18n'
import { notifyError } from '../../../auth/AuthProvider'
import type { OrganizationDto } from './organizationTypes'
import { getOrganization } from './organizationApi'
import { getLanguageLabel, getOrganizationTypeLabel } from './organizationUi'

export function OrganizationShowPage() {
  const t = useTranslate()
  const navigate = useNavigate()
  const { id } = useParams()
  const [organization, setOrganization] = useState<OrganizationDto | null>(null)

  useEffect(() => {
    if (id === undefined) {
      return
    }

    const organizationId = id

    async function loadOrganization() {
      try {
        setOrganization(await getOrganization(organizationId))
      } catch (error) {
        notifyError(error, t('shared.errors.generic'))
      }
    }

    void loadOrganization()
  }, [id, t])

  return (
    <main className="page">
      <Group justify="space-between" mb="sm">
        <h1 className="page-title">{t('resources.iam.organizations.pages.show')}</h1>
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
          <Text size="sm" c="dimmed">{t('shared.common.loading')}</Text>
        ) : (
          <SimpleGrid cols={{ base: 1, sm: 2 }} spacing="md">
            <Field label={t('resources.iam.organizations.fields.type')}>
              {getOrganizationTypeLabel(organization.type, t)}
            </Field>
            <Field label={t('resources.iam.organizations.fields.code')}>{organization.code}</Field>
            <Field label={t('resources.iam.organizations.fields.name')}>{organization.name}</Field>
            <Field label={t('resources.iam.organizations.fields.defaultLanguage')}>
              {getLanguageLabel(organization.defaultLanguage, t)}
            </Field>
            <Field label={t('resources.iam.organizations.fields.description')}>
              {organization.description ?? ''}
            </Field>
            <Field label={t('resources.iam.organizations.fields.isActive')}>
              <Badge color={organization.isActive ? 'green' : 'gray'} variant="light">
                {organization.isActive ? t('shared.status.active') : t('shared.status.inactive')}
              </Badge>
            </Field>
          </SimpleGrid>
        )}
      </Paper>
    </main>
  )
}

type FieldProps = {
  children: ReactNode
  label: string
}

function Field({ children, label }: FieldProps) {
  return (
    <Stack gap={2}>
      <Text size="xs" c="dimmed" fw={700}>{label}</Text>
      <Text size="sm">{children}</Text>
    </Stack>
  )
}
