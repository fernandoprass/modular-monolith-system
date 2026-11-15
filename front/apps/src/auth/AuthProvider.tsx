import { createContext, useCallback, useContext, useMemo, useRef, useState } from 'react'
import type { PropsWithChildren } from 'react'

import { useToast } from '../app/ToastProvider'
import { API_PATHS } from '../data/apiPaths'
import { getIamJson, postIamJson } from '../data/httpClient'
import { getApiErrorText, unwrapResult } from '../data/result'
import { normalizeLanguageCode } from '../shared/languages'
import type { PermissionCode } from '../shared/permissions'
import { tokenStorage, type AuthSession, type StoredUser } from './tokenStorage'

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
  updateProfile: (userId: string, profile: Pick<StoredUser, 'fullName' | 'language'>, session: AuthSession) => boolean
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
  const loginAttempt = useRef(0)
  const [user, setUser] = useState<StoredUser | null>(() => tokenStorage.getUser())
  const [permissions, setPermissions] = useState<PermissionCode[]>(() => tokenStorage.getPermissions())

  const logout = useCallback(() => {
    loginAttempt.current += 1
    tokenStorage.clearAll()
    setUser(null)
    setPermissions([])
  }, [])

  const login = useCallback(async (request: LoginRequest) => {
    const attempt = ++loginAttempt.current
    tokenStorage.clearAll()
    setUser(null)
    setPermissions([])
    let session = tokenStorage.getSession()

    try {
      const loginResponse = unwrapResult<LoginResponse>(
        await postIamJson(API_PATHS.iam.authentication.login, {
          email: request.email,
          password: request.password,
        }),
      )
      if (!tokenStorage.isCurrentSession(session)) return
      const storedUser = toStoredUser(loginResponse)

      tokenStorage.setToken(loginResponse.token)
      session = tokenStorage.getSession()

      const loadedPermissions = unwrapResult<PermissionCode[]>(
        await getIamJson(API_PATHS.iam.userAccess.userPermissionCodes(storedUser.id)),
      )

      if (!tokenStorage.isCurrentSession(session)) return
      tokenStorage.setUser(storedUser)
      tokenStorage.setPermissions(loadedPermissions)
      setUser(storedUser)
      setPermissions(loadedPermissions)
    } catch (error) {
      if (attempt !== loginAttempt.current) return
      if (tokenStorage.getToken() !== null && !tokenStorage.isCurrentSession(session)) return
      tokenStorage.clearAll()
      setUser(null)
      setPermissions([])
      throw error
    }
  }, [])

  const updateProfile = useCallback((userId: string, profile: Pick<StoredUser, 'fullName' | 'language'>, session: AuthSession) => {
    const updatedUser = tokenStorage.updateProfile(userId, profile, session)
    if (updatedUser === null) return false

    setUser(updatedUser)
    return true
  }, [])

  const value = useMemo<AuthContextValue>(() => ({
    isAuthenticated: user !== null && tokenStorage.getToken() !== null,
    login,
    logout,
    permissions,
    updateProfile,
    user,
  }), [login, logout, permissions, updateProfile, user])

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
