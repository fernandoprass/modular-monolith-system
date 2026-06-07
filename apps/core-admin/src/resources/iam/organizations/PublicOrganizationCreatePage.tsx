import { Button, Card, CardContent, Link, MenuItem, TextField } from '@mui/material'
import { useState } from 'react'
import type { FormEvent } from 'react'

import { APP_CONSTANTS } from '../../../app/appConstants'
import { API_PATHS } from '../../../data/apiPaths'
import { getApiErrorText, postJson, unwrapResult } from '../../../data/httpClient'
import { LANGUAGE_CODES, languageChoices } from '../../../shared/languages'
import {
  ORGANIZATION_TYPES,
  organizationTypeChoices,
} from './organizationTypes'
import type { OrganizationCreateRequest, OrganizationDto } from './organizationTypes'

const PUBLIC_FORM_LABELS = {
  adminEmail: 'Admin email',
  adminName: 'Admin name',
  adminPassword: 'Admin password',
  code: 'Code',
  defaultLanguage: 'Default language',
  description: 'Description',
  name: 'Name',
  signIn: 'Back to sign in',
  submit: 'Create organization',
  success: 'Organization created. You can sign in now.',
  title: 'Create organization',
  type: 'Type',
} as const

const PUBLIC_ORGANIZATION_TYPE_LABELS = {
  [ORGANIZATION_TYPES.company]: 'Company',
  [ORGANIZATION_TYPES.individual]: 'Individual',
} as const

const PUBLIC_LANGUAGE_LABELS = {
  [LANGUAGE_CODES.english]: 'English',
  [LANGUAGE_CODES.portugueseBrazil]: 'Portuguese - Brazil',
  [LANGUAGE_CODES.spanish]: 'Spanish',
} as const

function toCreateRequest(form: PublicOrganizationCreateForm): OrganizationCreateRequest {
  return {
    Type: form.type,
    Name: form.name,
    Code: form.code,
    Description: form.description,
    DefaultLanguage: form.defaultLanguage,
    User: {
      Name: form.userName,
      Email: form.userEmail,
      Password: form.userPassword,
    },
  }
}

type PublicOrganizationCreateForm = {
  type: number
  code: string
  name: string
  description: string
  defaultLanguage: string
  userName: string
  userEmail: string
  userPassword: string
}

export function PublicOrganizationCreatePage() {
  const [form, setForm] = useState<PublicOrganizationCreateForm>({
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

  function setField<TField extends keyof PublicOrganizationCreateForm>(
    field: TField,
    value: PublicOrganizationCreateForm[TField],
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
      const response = await postJson(API_PATHS.iam.organizations.list, toCreateRequest(form))
      unwrapResult<OrganizationDto>(response)
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
          <h2>{PUBLIC_FORM_LABELS.title}</h2>
          {isSuccess ? (
            <div>
              <p className="public-register-success">{PUBLIC_FORM_LABELS.success}</p>
              <Link href="/login">
                {PUBLIC_FORM_LABELS.signIn}
              </Link>
            </div>
          ) : (
            <form onSubmit={handleSubmit}>
              <TextField
                fullWidth
                label={PUBLIC_FORM_LABELS.type}
                margin="normal"
                onChange={(event) => setField('type', Number(event.target.value))}
                required
                select
                value={form.type}
              >
                {organizationTypeChoices.map((choice) => (
                  <MenuItem key={choice.id} value={choice.id}>
                    {PUBLIC_ORGANIZATION_TYPE_LABELS[choice.id]}
                  </MenuItem>
                ))}
              </TextField>
              {isCompany && (
                <>
                  <TextField
                    fullWidth
                    label={PUBLIC_FORM_LABELS.code}
                    margin="normal"
                    onChange={(event) => setField('code', event.target.value)}
                    required
                    value={form.code}
                  />
                  <TextField
                    fullWidth
                    label={PUBLIC_FORM_LABELS.name}
                    margin="normal"
                    onChange={(event) => setField('name', event.target.value)}
                    required
                    value={form.name}
                  />
                </>
              )}
              <TextField
                fullWidth
                label={PUBLIC_FORM_LABELS.description}
                margin="normal"
                onChange={(event) => setField('description', event.target.value)}
                required
                value={form.description}
              />
              <TextField
                fullWidth
                label={PUBLIC_FORM_LABELS.defaultLanguage}
                margin="normal"
                onChange={(event) => setField('defaultLanguage', event.target.value)}
                required
                select
                value={form.defaultLanguage}
              >
                {languageChoices.map((choice) => (
                  <MenuItem key={choice.id} value={choice.id}>
                    {PUBLIC_LANGUAGE_LABELS[choice.id]}
                  </MenuItem>
                ))}
              </TextField>
              <TextField
                fullWidth
                label={PUBLIC_FORM_LABELS.adminName}
                margin="normal"
                onChange={(event) => setField('userName', event.target.value)}
                required
                value={form.userName}
              />
              <TextField
                fullWidth
                label={PUBLIC_FORM_LABELS.adminEmail}
                margin="normal"
                onChange={(event) => setField('userEmail', event.target.value)}
                required
                type="email"
                value={form.userEmail}
              />
              <TextField
                fullWidth
                label={PUBLIC_FORM_LABELS.adminPassword}
                margin="normal"
                onChange={(event) => setField('userPassword', event.target.value)}
                required
                type="password"
                value={form.userPassword}
              />
              {error.length > 0 && <p className="public-register-error">{error}</p>}
              <Button disabled={isSubmitting} fullWidth type="submit" variant="contained">
                {PUBLIC_FORM_LABELS.submit}
              </Button>
              <Link className="login-signup-link" href="/login">
                {PUBLIC_FORM_LABELS.signIn}
              </Link>
            </form>
          )}
        </CardContent>
      </Card>
    </main>
  )
}
