import { Button, Card, CardContent, Link, MenuItem, TextField } from '@mui/material'
import { useState } from 'react'
import type { FormEvent } from 'react'

import { APP_CONSTANTS } from '../../../app/appConstants'
import { enMessages } from '../../../app/i18n/en'
import { APP_ROUTES } from '../../../app/routes'
import { getApiErrorText } from '../../../data/httpClient'
import { LANGUAGE_CODES, languageChoices } from '../../../shared/languages'
import {
  ORGANIZATION_TYPES,
  organizationTypeChoices,
} from './organizationTypes'
import type { OrganizationCreateForm } from './organizationTypes'
import { createOrganization } from './organizationApi'

const PUBLIC_TEXT = enMessages.public.organizationRegistration
const SHARED_TEXT = enMessages.shared
const ORGANIZATION_TEXT = enMessages.resources.iam.organizations

const ORGANIZATION_TYPE_LABELS = {
  [ORGANIZATION_TYPES.company]: ORGANIZATION_TEXT.types.company,
  [ORGANIZATION_TYPES.individual]: ORGANIZATION_TEXT.types.individual,
} as const

const LANGUAGE_LABELS = {
  [LANGUAGE_CODES.english]: SHARED_TEXT.languages.en,
  [LANGUAGE_CODES.portugueseBrazil]: SHARED_TEXT.languages.ptBr,
  [LANGUAGE_CODES.spanish]: SHARED_TEXT.languages.es,
} as const

export function PublicOrganizationCreatePage() {
  const [form, setForm] = useState<OrganizationCreateForm>({
    type: ORGANIZATION_TYPES.company,
    code: '',
    name: '',
    description: '',
    defaultLanguage: LANGUAGE_CODES.english,
    userName: '',
    userEmail: '',
    userPassword: '',
  })
  const [error, setError] = useState('')
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
    setError('')
    setIsSubmitting(true)

    try {
      await createOrganization(form)
      setIsSuccess(true)
    } catch (submitError) {
      setError(getApiErrorText(submitError))
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <main className="login-page">
      <Card className="public-register-card">
        <CardContent>
          <h1>{APP_CONSTANTS.appName}</h1>
          <h2>{PUBLIC_TEXT.title}</h2>
          {isSuccess ? (
            <div>
              <p className="public-register-success">{PUBLIC_TEXT.messages.success}</p>
              <Link href={APP_ROUTES.login}>
                {PUBLIC_TEXT.actions.signIn}
              </Link>
            </div>
          ) : (
            <form onSubmit={handleSubmit}>
              <TextField
                fullWidth
                label={PUBLIC_TEXT.fields.type}
                margin="normal"
                onChange={(event) => setField('type', Number(event.target.value))}
                required
                select
                value={form.type}
              >
                {organizationTypeChoices.map((choice) => (
                  <MenuItem key={choice.id} value={choice.id}>
                    {ORGANIZATION_TYPE_LABELS[choice.id]}
                  </MenuItem>
                ))}
              </TextField>
              {isCompany && (
                <>
                  <TextField
                    fullWidth
                    label={PUBLIC_TEXT.fields.code}
                    margin="normal"
                    onChange={(event) => setField('code', event.target.value)}
                    required
                    value={form.code}
                  />
                  <TextField
                    fullWidth
                    label={PUBLIC_TEXT.fields.name}
                    margin="normal"
                    onChange={(event) => setField('name', event.target.value)}
                    required
                    value={form.name}
                  />
                </>
              )}
              <TextField
                fullWidth
                label={PUBLIC_TEXT.fields.description}
                margin="normal"
                onChange={(event) => setField('description', event.target.value)}
                required
                value={form.description}
              />
              <TextField
                fullWidth
                label={PUBLIC_TEXT.fields.defaultLanguage}
                margin="normal"
                onChange={(event) => setField('defaultLanguage', event.target.value)}
                required
                select
                value={form.defaultLanguage}
              >
                {languageChoices.map((choice) => (
                  <MenuItem key={choice.id} value={choice.id}>
                    {LANGUAGE_LABELS[choice.id]}
                  </MenuItem>
                ))}
              </TextField>
              <TextField
                fullWidth
                label={PUBLIC_TEXT.fields.adminName}
                margin="normal"
                onChange={(event) => setField('userName', event.target.value)}
                required
                value={form.userName}
              />
              <TextField
                fullWidth
                label={PUBLIC_TEXT.fields.adminEmail}
                margin="normal"
                onChange={(event) => setField('userEmail', event.target.value)}
                required
                type="email"
                value={form.userEmail}
              />
              <TextField
                fullWidth
                label={PUBLIC_TEXT.fields.adminPassword}
                margin="normal"
                onChange={(event) => setField('userPassword', event.target.value)}
                required
                type="password"
                value={form.userPassword}
              />
              {error.length > 0 && <p className="public-register-error">{error}</p>}
              <Button disabled={isSubmitting} fullWidth type="submit" variant="contained">
                {PUBLIC_TEXT.actions.submit}
              </Button>
              <Link className="login-signup-link" href={APP_ROUTES.login}>
                {PUBLIC_TEXT.actions.signIn}
              </Link>
            </form>
          )}
        </CardContent>
      </Card>
    </main>
  )
}
