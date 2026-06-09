import { useState } from 'react'
import type { FormEvent } from 'react'
import { Link, Navigate, useNavigate } from 'react-router-dom'

import { APP_CONSTANTS } from '../app/appConstants'
import { useTranslate } from '../app/i18n/i18n'
import { APP_ROUTES } from '../app/routes'
import { Button } from '../components/ui/button'
import { Card, CardContent } from '../components/ui/card'
import { Field } from '../components/ui/field'
import { Input } from '../components/ui/input'
import { useAuth, useNotifyError } from './AuthProvider'

export function LoginPage() {
  const t = useTranslate()
  const navigate = useNavigate()
  const notifyError = useNotifyError()
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
      <Card className="auth-card">
        <CardContent>
          <form onSubmit={handleSubmit}>
            <div className="form-stack">
              <div>
                <h1 className="auth-title">{APP_CONSTANTS.appName}</h1>
                <p className="auth-subtitle">{t('auth.login.title')}</p>
              </div>
              <Field label={t('auth.login.email')}>
                <Input
                  autoComplete="email"
                  onChange={(event) => setEmail(event.currentTarget.value)}
                  required
                  type="email"
                  value={email}
                />
              </Field>
              <Field label={t('auth.login.password')}>
                <Input
                  autoComplete="current-password"
                  onChange={(event) => setPassword(event.currentTarget.value)}
                  required
                  type="password"
                  value={password}
                />
              </Field>
              <Button disabled={isSubmitting} type="submit">
                {t('auth.login.submit')}
              </Button>
              <Link className="text-link text-center" to={APP_ROUTES.registerOrganization}>
                {t('auth.login.signUp')}
              </Link>
            </div>
          </form>
        </CardContent>
      </Card>
    </main>
  )
}
