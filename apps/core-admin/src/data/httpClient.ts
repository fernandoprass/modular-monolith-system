import { fetchUtils } from 'react-admin'

import { CONFIG } from '../app/config'
import { tokenStorage } from '../auth/tokenStorage'

type ApiResult<TData> = {
  data: TData
  messages: string[]
  isSuccess: boolean
  title: string | null
}

function getHeaders(): Headers {
  const headers = new Headers({ 'Content-Type': 'application/json' })
  const token = tokenStorage.getToken()

  if (token !== null) {
    headers.set('Authorization', `Bearer ${token}`)
  }

  return headers
}

export async function postJson(path: string, body: unknown): Promise<unknown> {
  const response = await fetchUtils.fetchJson(`${CONFIG.coreApiUrl}${path}`, {
    method: 'POST',
    body: JSON.stringify(body),
    headers: getHeaders(),
  })

  return response.json
}

export function unwrapResult<TData>(response: unknown): TData {
  const result = response as ApiResult<TData>

  if (!result.isSuccess) {
    throw new Error(result.messages.join('\n'))
  }

  return result.data
}
