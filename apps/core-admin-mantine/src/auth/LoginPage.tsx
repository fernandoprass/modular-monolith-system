import { Anchor, Button, Paper, PasswordInput, Stack, TextInput } from '@mantine/core'
import { useState } from 'react'
import type { FormEvent } from 'react'
import { Link, Navigate, useNavigate } from 'react-router-dom'

import { APP_CONSTANTS } from '../app/appConstants'
import { APP_ROUTES } from '../app/routes'
import { useTranslate } from '../app/i18n/i18n'
import { useAuth, notifyError } from './AuthProvider'

export function LoginPage() {
  const t = useTranslate()
  const navigate = useNavigate()
  const { isAuthenticated, login } = useAuth()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  if (isAuthenticated) {
    return <Navigate to={APP_ROUTES.dashboard} replace />
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setIsSubmitting(true)

    try {
      await login({ email, password })
      navigate(APP_ROUTES.dashboard)
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <main className="auth-page">
      <Paper className="auth-card" shadow="xs" p="md" withBorder>
        <form onSubmit={handleSubmit}>
          <Stack gap="sm">
            <div>
              <h1 className="auth-title">{APP_CONSTANTS.appName}</h1>
              <p className="auth-subtitle">{t('auth.login.title')}</p>
            </div>
            <TextInput
              autoComplete="email"
              label={t('auth.login.email')}
              onChange={(event) => setEmail(event.currentTarget.value)}
              required
              type="email"
              value={email}
            />
            <PasswordInput
              autoComplete="current-password"
              label={t('auth.login.password')}
              onChange={(event) => setPassword(event.currentTarget.value)}
              required
              value={password}
            />
            <Button loading={isSubmitting} type="submit" fullWidth>
              {t('auth.login.submit')}
            </Button>
            <Anchor component={Link} to={APP_ROUTES.registerOrganization} ta="center" size="sm">
              {t('auth.login.signUp')}
            </Anchor>
          </Stack>
        </form>
      </Paper>
    </main>
  )
}
