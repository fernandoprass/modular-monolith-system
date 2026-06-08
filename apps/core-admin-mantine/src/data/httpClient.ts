import { CONFIG } from '../app/config'
import { tokenStorage } from '../auth/tokenStorage'
import { ApiResultError, isApiResult } from './result'

type RequestOptions = {
  body?: unknown
  method: string
  query?: URLSearchParams
}

function buildUrl(path: string, query?: URLSearchParams): string {
  const queryString = query?.toString()
  return queryString === undefined || queryString.length === 0
    ? `${CONFIG.coreApiUrl}${path}`
    : `${CONFIG.coreApiUrl}${path}?${queryString}`
}

function getHeaders(): Headers {
  const headers = new Headers({ 'Content-Type': 'application/json' })
  const token = tokenStorage.getToken()

  if (token !== null) {
    headers.set('Authorization', `Bearer ${token}`)
  }

  return headers
}

async function readBody(response: Response): Promise<unknown> {
  const text = await response.text()

  if (text.length === 0) {
    return null
  }

  try {
    return JSON.parse(text) as unknown
  } catch {
    if (response.ok) {
      return text
    }

    return null
  }
}

async function request(path: string, options: RequestOptions): Promise<unknown> {
  const response = await fetch(buildUrl(path, options.query), {
    body: options.body === undefined ? undefined : JSON.stringify(options.body),
    headers: getHeaders(),
    method: options.method,
  })
  const body = await readBody(response)

  if (response.status === 401 || response.status === 403) {
    tokenStorage.clearAll()
  }

  if (!response.ok) {
    if (isApiResult(body)) {
      throw new ApiResultError(body.title, body.messages, response.status)
    }

    throw new ApiResultError(response.statusText, [], response.status)
  }

  return body
}

export function getJson(path: string): Promise<unknown> {
  return request(path, { method: 'GET' })
}

export function getJsonWithQuery(path: string, query: URLSearchParams): Promise<unknown> {
  return request(path, { method: 'GET', query })
}

export function postJson(path: string, body: unknown): Promise<unknown> {
  return request(path, { body, method: 'POST' })
}

export function putJson(path: string, body: unknown): Promise<unknown> {
  return request(path, { body, method: 'PUT' })
}

export function patchJson(path: string, body: unknown): Promise<unknown> {
  return request(path, { body, method: 'PATCH' })
}

export function deleteJson(path: string): Promise<unknown> {
  return request(path, { method: 'DELETE' })
}
