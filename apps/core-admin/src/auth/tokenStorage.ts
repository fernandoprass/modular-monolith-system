import { STORAGE_KEYS } from './storageKeys'

export type StoredUser = {
  id: string
  fullName: string
}

export const tokenStorage = {
  getToken(): string | null {
    return localStorage.getItem(STORAGE_KEYS.authToken)
  },

  setToken(token: string): void {
    localStorage.setItem(STORAGE_KEYS.authToken, token)
  },

  clearToken(): void {
    localStorage.removeItem(STORAGE_KEYS.authToken)
  },

  getUser(): StoredUser | null {
    const value = localStorage.getItem(STORAGE_KEYS.authUser)

    if (value === null) {
      return null
    }

    return JSON.parse(value) as StoredUser
  },

  setUser(user: StoredUser): void {
    localStorage.setItem(STORAGE_KEYS.authUser, JSON.stringify(user))
  },

  clearUser(): void {
    localStorage.removeItem(STORAGE_KEYS.authUser)
  },
}
