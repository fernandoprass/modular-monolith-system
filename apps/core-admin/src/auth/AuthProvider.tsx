import { createContext, useCallback, useContext, useMemo, useState } from 'react'
import type { PropsWithChildren } from 'react'

import { useToast } from '../app/ToastProvider'
import { API_PATHS } from '../data/apiPaths'
import { getIamJson, postIamJson } from '../data/httpClient'
import { getApiErrorText, unwrapResult } from '../data/result'
import { normalizeLanguageCode } from '../shared/languages'
import type { PermissionCode } from '../shared/permissions'
import { tokenStorage, type StoredUser } from './tokenStorage'

type LoginResponse = {
  token: string
  expiresAt: string
  user: {
    email: string
    id: string
    isOrganizationAdmin: boolean
    isSystemAdmin: boolean
    language: string
    name: string
    organizationId: string
    organizationName: string
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
  permissions: PermissionCode[]
  user: StoredUser | null
}

const AuthContext = createContext<AuthContextValue | null>(null)

function toStoredUser(response: LoginResponse): StoredUser {
  return {
    email: response.user.email,
    fullName: response.user.name,
    id: response.user.id,
    isOrganizationAdmin: response.user.isOrganizationAdmin,
    isSystemAdmin: response.user.isSystemAdmin,
    language: normalizeLanguageCode(response.user.language),
    organizationId: response.user.organizationId,
    organizationName: response.user.organizationName,
  }
}

export function AuthProvider({ children }: PropsWithChildren) {
  const [user, setUser] = useState<StoredUser | null>(() => tokenStorage.getUser())
  const [permissions, setPermissions] = useState<PermissionCode[]>(() => tokenStorage.getPermissions())

  const logout = useCallback(() => {
    tokenStorage.clearAll()
    setUser(null)
    setPermissions([])
  }, [])

  const login = useCallback(async (request: LoginRequest) => {
    try {
      const loginResponse = unwrapResult<LoginResponse>(
        await postIamJson(API_PATHS.iam.authentication.login, {
          email: request.email,
          password: request.password,
        }),
      )
      const storedUser = toStoredUser(loginResponse)

      tokenStorage.setToken(loginResponse.token)

      const loadedPermissions = unwrapResult<PermissionCode[]>(
        await getIamJson(API_PATHS.iam.userAccess.userPermissionCodes(storedUser.id)),
      )

      tokenStorage.setUser(storedUser)
      tokenStorage.setPermissions(loadedPermissions)
      setUser(storedUser)
      setPermissions(loadedPermissions)
    } catch (error) {
      tokenStorage.clearAll()
      setUser(null)
      setPermissions([])
      throw error
    }
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
