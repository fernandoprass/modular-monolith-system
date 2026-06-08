import { Button, Group, Modal, Stack, TextInput } from '@mantine/core'
import { notifications } from '@mantine/notifications'
import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'

import { useTranslate } from '../../../app/i18n/i18n'
import { notifyError } from '../../../auth/AuthProvider'
import { ORGANIZATION_REQUEST_FIELDS, type OrganizationDto } from './organizationTypes'
import { updateOrganizationCode } from './organizationApi'

type OrganizationCodeEditModalProps = {
  isOpen: boolean
  onClose: () => void
  onSaved: () => Promise<void>
  organization: OrganizationDto
}

export function OrganizationCodeEditModal({
  isOpen,
  onClose,
  onSaved,
  organization,
}: OrganizationCodeEditModalProps) {
  const t = useTranslate()
  const [code, setCode] = useState(organization.code)
  const [isSaving, setIsSaving] = useState(false)

  useEffect(() => {
    if (isOpen) {
      setCode(organization.code)
    }
  }, [isOpen, organization.code])

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setIsSaving(true)

    try {
      await updateOrganizationCode(organization.id, {
        [ORGANIZATION_REQUEST_FIELDS.code]: code,
      })
      notifications.show({
        color: 'green',
        message: t('resources.iam.organizations.notifications.codeUpdated'),
      })
      await onSaved()
      onClose()
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    } finally {
      setIsSaving(false)
    }
  }

  return (
    <Modal
      opened={isOpen}
      onClose={onClose}
      title={t('resources.iam.organizations.actions.editCode')}
      size="sm"
    >
      <form onSubmit={handleSubmit}>
        <Stack gap="sm">
          <TextInput
            autoFocus
            label={t('resources.iam.organizations.fields.code')}
            onChange={(event) => setCode(event.currentTarget.value)}
            required
            value={code}
          />
          <Group justify="flex-end">
            <Button variant="default" onClick={onClose}>
              {t('shared.actions.cancel')}
            </Button>
            <Button loading={isSaving} type="submit">
              {t('shared.actions.save')}
            </Button>
          </Group>
        </Stack>
      </form>
    </Modal>
  )
}
