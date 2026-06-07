import { Button, Card, CardContent, Link, TextField } from '@mui/material'
import { useState } from 'react'
import type { FormEvent } from 'react'
import { useLogin, useNotify, useTranslate } from 'react-admin'
import { Link as RouterLink } from 'react-router-dom'

import { APP_CONSTANTS } from '../app/appConstants'
import { getApiErrorText } from '../data/httpClient'

export function LoginPage() {
  const login = useLogin()
  const notify = useNotify()
  const translate = useTranslate()
  const [email, setEmail] = useState('alan.turing@enigma.org')
  const [password, setPassword] = useState('Password123!')
  const [isSubmitting, setIsSubmitting] = useState(false)

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setIsSubmitting(true)

    try {
      await login({ email, password })
    } catch (error) {
      notify(getApiErrorText(error), { type: 'error' })
      setIsSubmitting(false)
    }
  }

  return (
    <main className="login-page">
      <Card className="login-card">
        <CardContent>
          <h1>{APP_CONSTANTS.appName}</h1>
          <form onSubmit={handleSubmit}>
            <TextField
              autoComplete="email"
              autoFocus
              fullWidth
              label={translate('auth.login.email')}
              margin="normal"
              name="email"
              onChange={(event) => setEmail(event.target.value)}
              required
              type="email"
              value={email}
            />
            <TextField
              autoComplete="current-password"
              fullWidth
              label={translate('auth.login.password')}
              margin="normal"
              name="password"
              onChange={(event) => setPassword(event.target.value)}
              required
              type="password"
              value={password}
            />
            <Button
              disabled={isSubmitting}
              fullWidth
              type="submit"
              variant="contained"
            >
              {translate('auth.login.submit')}
            </Button>
            <p> </p>
            <Link className="login-signup-link" component={RouterLink} to="/register-organization">
              {translate('auth.login.signUp')}
            </Link>
          </form>
        </CardContent>
      </Card>
    </main>
  )
}
