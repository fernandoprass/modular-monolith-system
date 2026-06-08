import EditIcon from '@mui/icons-material/Edit'
import { Button, Dialog, DialogActions, DialogContent, DialogTitle, TextField } from '@mui/material'
import { useState } from 'react'
import type { FormEvent } from 'react'
import { useNotify, useRecordContext, useRefresh, useTranslate } from 'react-admin'

import { API_PATHS } from '../../../data/apiPaths'
import { ensureResultSuccess, getApiErrorText, patchJson } from '../../../data/httpClient'
import { ORGANIZATION_REQUEST_FIELDS, type OrganizationDto } from './organizationTypes'

export function OrganizationCodeEditButton() {
  const record = useRecordContext<OrganizationDto>()
  const notify = useNotify()
  const refresh = useRefresh()
  const translate = useTranslate()
  const [isOpen, setIsOpen] = useState(false)
  const [code, setCode] = useState(record?.code ?? '')
  const [isSaving, setIsSaving] = useState(false)

  if (record === undefined) {
    return null
  }

  const organization = record

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setIsSaving(true)

    try {
      const response = await patchJson(API_PATHS.iam.organizations.code(organization.id), {
        [ORGANIZATION_REQUEST_FIELDS.code]: code,
      })

      ensureResultSuccess(response)
      notify('resources.iam.organizations.notifications.codeUpdated', { type: 'success' })
      setIsOpen(false)
      refresh()
    } catch (error) {
      notify(getApiErrorText(error), { type: 'error' })
    } finally {
      setIsSaving(false)
    }
  }

  return (
    <>
      <Button
        onClick={() => {
          setCode(organization.code)
          setIsOpen(true)
        }}
        size="small"
        startIcon={<EditIcon />}
      >
        {translate('resources.iam.organizations.actions.editCode')}
      </Button>
      <Dialog fullWidth maxWidth="xs" onClose={() => setIsOpen(false)} open={isOpen}>
        <form onSubmit={handleSubmit}>
          <DialogTitle>{translate('resources.iam.organizations.actions.editCode')}</DialogTitle>
          <DialogContent>
            <TextField
              autoFocus
              fullWidth
              label={translate('resources.iam.organizations.fields.code')}
              margin="normal"
              onChange={(event) => setCode(event.target.value)}
              required
              value={code}
            />
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setIsOpen(false)}>
              {translate('shared.actions.cancel')}
            </Button>
            <Button disabled={isSaving} type="submit" variant="contained">
              {translate('shared.actions.save')}
            </Button>
          </DialogActions>
        </form>
      </Dialog>
    </>
  )
}
