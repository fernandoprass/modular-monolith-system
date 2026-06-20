import { zodResolver } from '@hookform/resolvers/zod'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { Link, Navigate, useNavigate } from 'react-router-dom'
import { z } from 'zod'

import { APP_CONSTANTS } from '../app/appConstants'
import { useTranslate } from '../app/i18n/i18n'
import { APP_ROUTES } from '../app/routes'
import { Button } from '../components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card'
import { Field, FieldGroup, FieldLabel } from '../components/ui/form'
import { Input } from '../components/ui/input'
import { useAuth, useNotifyError } from './AuthProvider'

type LoginForm = {
  email: string
  password: string
}

const loginSchema = z.object({
  email: z.string().trim().email(),
  password: z.string().min(1),
})

export function LoginPage() {
  const t = useTranslate()
  const navigate = useNavigate()
  const notifyError = useNotifyError()
  const { isAuthenticated, login } = useAuth()
  const [isSubmitting, setIsSubmitting] = useState(false)
  const {
    handleSubmit,
    register,
  } = useForm<LoginForm>({
    defaultValues: {
      email: 'admin@saas.com',
      password: 'Password123!',
    },
    resolver: zodResolver(loginSchema),
  })

  async function handleLogin(value: LoginForm) {
    setIsSubmitting(true)

    try {
      await login(value)
      navigate(APP_ROUTES.dashboard)
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    } finally {
      setIsSubmitting(false)
    }
  }

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
          <form onSubmit={handleSubmit(handleLogin)}>
            <FieldGroup>
              <Field>
                <FieldLabel htmlFor="email">{t('auth.login.email')}</FieldLabel>
                <Input
                  autoComplete="email"
                  id="email"
                  required
                  type="email"
                  {...register('email')}
                />
              </Field>
              <Field>
                <FieldLabel htmlFor="password">{t('auth.login.password')}</FieldLabel>
                <Input
                  autoComplete="current-password"
                  id="password"
                  required
                  type="password"
                  {...register('password')}
                />
              </Field>
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
