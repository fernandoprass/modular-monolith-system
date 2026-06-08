import { createContext, useCallback, useContext, useMemo, useState } from 'react'
import type { PropsWithChildren } from 'react'

import { useToast } from '../app/ToastProvider'
import { API_PATHS } from '../data/apiPaths'
import { getJson, postJson } from '../data/httpClient'
import { getApiErrorText, unwrapResult } from '../data/result'
import type { PermissionDto } from '../shared/permissions'
import { tokenStorage, type StoredUser } from './tokenStorage'

type LoginResponse = {
  token: string
  expiresAt: string
  user: {
    email: string
    id: string
    isSystemAdmin: boolean
    name: string
    organizationId: string
  }
}

type LoginRequest = {
  email: string
  password: string
}

type AuthContextValue = {
  isAuthenticated: boolean
  login: (request: LoginRequest) => Promise<void>
  logout: () => void
  permissions: PermissionDto[]
  user: StoredUser | null
}

const AuthContext = createContext<AuthContextValue | null>(null)

function toStoredUser(response: LoginResponse): StoredUser {
  return {
    email: response.user.email,
    fullName: response.user.name,
    id: response.user.id,
    isSystemAdmin: response.user.isSystemAdmin,
    organizationId: response.user.organizationId,
  }
}

export function AuthProvider({ children }: PropsWithChildren) {
  const [user, setUser] = useState<StoredUser | null>(() => tokenStorage.getUser())
  const [permissions, setPermissions] = useState<PermissionDto[]>(() => tokenStorage.getPermissions())

  const logout = useCallback(() => {
    tokenStorage.clearAll()
    setUser(null)
    setPermissions([])
  }, [])

  const login = useCallback(async (request: LoginRequest) => {
    const loginResponse = unwrapResult<LoginResponse>(
      await postJson(API_PATHS.iam.users.login, {
        email: request.email,
        password: request.password,
      }),
    )
    const storedUser = toStoredUser(loginResponse)

    tokenStorage.setToken(loginResponse.token)
    tokenStorage.setUser(storedUser)

    const loadedPermissions = unwrapResult<PermissionDto[]>(
      await getJson(API_PATHS.iam.roles.userPermissions(storedUser.id)),
    )

    tokenStorage.setPermissions(loadedPermissions)
    setUser(storedUser)
    setPermissions(loadedPermissions)
  }, [])

  const value = useMemo<AuthContextValue>(() => ({
    isAuthenticated: user !== null && tokenStorage.getToken() !== null,
    login,
    logout,
    permissions,
    user,
  }), [login, logout, permissions, user])

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext)

  if (context === null) {
    throw new Error('Auth context missing.')
  }

  return context
}

export function useNotifyError(): (error: unknown, fallback: string) => void {
  const { showError } = useToast()

  return useCallback((error: unknown, fallback: string) => {
    showError(getApiErrorText(error, fallback))
  }, [showError])
}
