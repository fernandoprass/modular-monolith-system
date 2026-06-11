import { useForm } from '@tanstack/react-form'
import { useState } from 'react'
import { Link, Navigate, useNavigate } from 'react-router-dom'

import { APP_CONSTANTS } from '../app/appConstants'
import { useTranslate } from '../app/i18n/i18n'
import { APP_ROUTES } from '../app/routes'
import { Button } from '../components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card'
import { Field, FieldGroup, FieldLabel } from '../components/ui/form'
import { Input } from '../components/ui/input'
import { useAuth, useNotifyError } from './AuthProvider'

export function LoginPage() {
  const t = useTranslate()
  const navigate = useNavigate()
  const notifyError = useNotifyError()
  const { isAuthenticated, login } = useAuth()
  const [isSubmitting, setIsSubmitting] = useState(false)
  const form = useForm({
    defaultValues: {
      email: 'admin@saas.com',
      password: 'Password123!',
    },
    onSubmit: async ({ value }) => {
      setIsSubmitting(true)

      try {
        await login(value)
        navigate(APP_ROUTES.dashboard)
      } catch (error) {
        notifyError(error, t('shared.errors.generic'))
      } finally {
        setIsSubmitting(false)
      }
    },
  })

  if (isAuthenticated) {
    return <Navigate to={APP_ROUTES.dashboard} replace />
  }

  return (
    <main className="auth-page">
      <Card className="auth-card">
        <CardHeader>
          <CardTitle>{APP_CONSTANTS.appName}</CardTitle>
          <CardDescription>{t('auth.login.title')}</CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={(event) => {
            event.preventDefault()
            void form.handleSubmit()
          }}>
            <FieldGroup>
              <form.Field name="email">
                {(field) => (
                  <Field>
                    <FieldLabel htmlFor={field.name}>{t('auth.login.email')}</FieldLabel>
                    <Input
                      autoComplete="email"
                      id={field.name}
                      name={field.name}
                      onBlur={field.handleBlur}
                      onChange={(event) => field.handleChange(event.currentTarget.value)}
                      required
                      type="email"
                      value={field.state.value}
                    />
                  </Field>
                )}
              </form.Field>
              <form.Field name="password">
                {(field) => (
                  <Field>
                    <FieldLabel htmlFor={field.name}>{t('auth.login.password')}</FieldLabel>
                    <Input
                      autoComplete="current-password"
                      id={field.name}
                      name={field.name}
                      onBlur={field.handleBlur}
                      onChange={(event) => field.handleChange(event.currentTarget.value)}
                      required
                      type="password"
                      value={field.state.value}
                    />
                  </Field>
                )}
              </form.Field>
              <Button disabled={isSubmitting} type="submit">
                {t('auth.login.submit')}
              </Button>
              <Link className="text-link text-center" to={APP_ROUTES.registerOrganization}>
                {t('auth.login.signUp')}
              </Link>
            </FieldGroup>
          </form>
        </CardContent>
      </Card>
    </main>
  )
}
