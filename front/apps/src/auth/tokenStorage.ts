import type { PermissionCode } from '../shared/permissions'
import { STORAGE_KEYS } from './storageKeys'
import { normalizeLanguageCode } from '../shared/languages'

let sessionVersion = 0

export type AuthSession = { version: number; token: string | null }

export type StoredUser = {
  email: string
  fullName: string
  id: string
  isOrganizationAdmin: boolean
  isSystemAdmin: boolean
  language: string
  organizationId: string
  organizationName: string
}

function parseStoredValue<TValue>(value: string | null, clearValue: () => void): TValue | null {
  if (value === null) {
    return null
  }

  try {
    return JSON.parse(value) as TValue
  } catch {
    clearValue()
    return null
  }
}

export const tokenStorage = {
  getSession(): AuthSession {
    return { version: sessionVersion, token: this.getToken() }
  },

  isCurrentSession(session: AuthSession): boolean {
    return session.version === sessionVersion && session.token === this.getToken()
  },

  getToken(): string | null {
    return localStorage.getItem(STORAGE_KEYS.authToken)
  },

  setToken(token: string): void {
    sessionVersion += 1
    localStorage.setItem(STORAGE_KEYS.authToken, token)
  },

  clearToken(): void {
    sessionVersion += 1
    localStorage.removeItem(STORAGE_KEYS.authToken)
  },

  getUser(): StoredUser | null {
    return parseStoredValue<StoredUser>(
      localStorage.getItem(STORAGE_KEYS.authUser),
      () => localStorage.removeItem(STORAGE_KEYS.authUser),
    )
  },

  setUser(user: StoredUser): void {
    localStorage.setItem(STORAGE_KEYS.authUser, JSON.stringify(user))
  },

  updateProfile(userId: string, profile: Pick<StoredUser, 'fullName' | 'language'>, session: AuthSession): StoredUser | null {
    const user = this.getUser()
    if (!this.isCurrentSession(session) || session.token === null || user?.id !== userId) return null

    const updatedUser = { ...user, ...profile, language: normalizeLanguageCode(profile.language) }
    this.setUser(updatedUser)
    return updatedUser
  },

  clearUser(): void {
    localStorage.removeItem(STORAGE_KEYS.authUser)
  },

  getPermissions(): PermissionCode[] {
    return parseStoredValue<PermissionCode[]>(
      localStorage.getItem(STORAGE_KEYS.authPermissions),
      () => localStorage.removeItem(STORAGE_KEYS.authPermissions),
    ) ?? []
  },

  setPermissions(permissions: PermissionCode[]): void {
    localStorage.setItem(STORAGE_KEYS.authPermissions, JSON.stringify(permissions))
  },

  clearPermissions(): void {
    localStorage.removeItem(STORAGE_KEYS.authPermissions)
  },

  clearAll(): void {
    this.clearToken()
    this.clearUser()
    this.clearPermissions()
  },
}
