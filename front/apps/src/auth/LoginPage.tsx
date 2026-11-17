import { zodResolver } from '@hookform/resolvers/zod'
import { createValidationErrorMap } from '../shared/validation'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { Link, Navigate, useNavigate } from 'react-router-dom'
import { z } from 'zod'

import { APP_CONSTANTS } from '../app/appConstants'
import { useTranslate } from '../app/i18n/i18n'
import { APP_ROUTES } from '../app/routes'
import { Button } from '../components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card'
import { Field, FieldError, FieldGroup, FieldLabel } from '../components/ui/form'
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
    formState: { errors },
    handleSubmit,
    register,
  } = useForm<LoginForm>({
    defaultValues: {
      email: 'admin@saas.com',
      password: 'Password123!',
    },
    resolver: zodResolver(loginSchema, { error: createValidationErrorMap(t) }),
  })

  async function handleLogin(value: LoginForm) {
    setIsSubmitting(true)

    try {
      await login(value)
      navigate(APP_ROUTES.dashboard)
    } catch (error) {
      notifyError(error, t('common.messages.errors.generic'))
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
          <CardDescription>{t('features.iam.actions.signIn')}</CardDescription>
        </CardHeader>
        <CardContent>
          <form noValidate onSubmit={handleSubmit(handleLogin)}>
            <FieldGroup>
              <Field data-invalid={errors.email !== undefined}>
                <FieldLabel htmlFor="email">{t('common.labels.email')}</FieldLabel>
                <Input
                  aria-invalid={errors.email !== undefined}
                  aria-describedby={errors.email ? 'LoginPage-email-error' : undefined}
                  autoComplete="email"
                  id="email"
                  required
                  type="email"
                  {...register('email')}
                />
                <FieldError id="LoginPage-email-error">{errors.email?.message}</FieldError>
              </Field>
              <Field data-invalid={errors.password !== undefined}>
                <FieldLabel htmlFor="password">{t('features.iam.labels.password')}</FieldLabel>
                <Input
                  aria-invalid={errors.password !== undefined}
                  aria-describedby={errors.password ? 'LoginPage-password-error' : undefined}
                  autoComplete="current-password"
                  id="password"
                  required
                  type="password"
                  {...register('password')}
                />
                <FieldError id="LoginPage-password-error">{errors.password?.message}</FieldError>
              </Field>
              <Button disabled={isSubmitting} type="submit">
                {t('features.iam.actions.signIn')}
              </Button>
              <Link className="text-link text-center" to={APP_ROUTES.registerOrganization}>
                {t('features.iam.messages.login.signUp')}
              </Link>
            </FieldGroup>
          </form>
        </CardContent>
      </Card>
    </main>
  )
}
