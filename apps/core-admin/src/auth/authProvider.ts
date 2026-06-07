import type { AuthProvider } from 'react-admin'

import { API_PATHS } from '../data/apiPaths'
import { getJson, postJson, unwrapResult } from '../data/httpClient'
import type { PermissionDto } from '../shared/permissions'
import { tokenStorage } from './tokenStorage'

type LoginParams = {
  username?: string
  email?: string
  password?: string
}

type LoginResponse = {
  token: string
  user: {
    id: string
    name: string
    email: string
  }
}

function toLoginParams(value: unknown): LoginParams {
  return value as LoginParams
}

export const authProvider: AuthProvider = {
  async login(params: unknown) {
    const loginParams = toLoginParams(params)
    const email = loginParams.email ?? loginParams.username ?? ''
    const password = loginParams.password ?? ''
    const response = await postJson(API_PATHS.iam.users.login, { email, password })
    const loginResponse = unwrapResult<LoginResponse>(response)

    tokenStorage.setToken(loginResponse.token)
    tokenStorage.setUser({
      id: loginResponse.user.id,
      fullName: loginResponse.user.name,
    })

    const permissionsResponse = await getJson(API_PATHS.iam.roles.userPermissions(loginResponse.user.id))
    const permissions = unwrapResult<PermissionDto[]>(permissionsResponse)

    tokenStorage.setPermissions(permissions)
  },

  async logout() {
    tokenStorage.clearPermissions()
    tokenStorage.clearToken()
    tokenStorage.clearUser()
  },

  async checkAuth() {
    if (tokenStorage.getToken() === null) {
      throw new Error('auth.required')
    }
  },

  async checkError(error: unknown) {
    const status = typeof error === 'object' && error !== null && 'status' in error
      ? error.status
      : undefined

    if (status === 401 || status === 403) {
      tokenStorage.clearToken()
      tokenStorage.clearUser()
      tokenStorage.clearPermissions()
      throw new Error('auth.invalid')
    }
  },

  async getIdentity() {
    const user = tokenStorage.getUser()

    if (user === null) {
      throw new Error('auth.required')
    }

    return user
  },

  async getPermissions() {
    return tokenStorage.getPermissions()
  },
}
