import { CONFIG } from '../app/config'
import { tokenStorage } from '../auth/tokenStorage'
import { ApiResultError, isApiResult } from './result'

type RequestOptions = {
  baseUrl: string
  body?: unknown
  method: string
  query?: URLSearchParams
}

function buildUrl(baseUrl: string, path: string, query?: URLSearchParams): string {
  const queryString = query?.toString()
  return queryString === undefined || queryString.length === 0
    ? `${baseUrl}${path}`
    : `${baseUrl}${path}?${queryString}`
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
    return response.ok ? text : null
  }
}

async function request(path: string, options: RequestOptions): Promise<unknown> {
  const response = await fetch(buildUrl(options.baseUrl, path, options.query), {
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
  return request(path, { baseUrl: CONFIG.apiBaseUrls.core, method: 'GET' })
}

export function getJsonWithQuery(path: string, query: URLSearchParams): Promise<unknown> {
  return request(path, { baseUrl: CONFIG.apiBaseUrls.core, method: 'GET', query })
}

export function postJson(path: string, body: unknown): Promise<unknown> {
  return request(path, { baseUrl: CONFIG.apiBaseUrls.core, body, method: 'POST' })
}

export function putJson(path: string, body: unknown): Promise<unknown> {
  return request(path, { baseUrl: CONFIG.apiBaseUrls.core, body, method: 'PUT' })
}

export function patchJson(path: string, body: unknown): Promise<unknown> {
  return request(path, { baseUrl: CONFIG.apiBaseUrls.core, body, method: 'PATCH' })
}

export function deleteJson(path: string): Promise<unknown> {
  return request(path, { baseUrl: CONFIG.apiBaseUrls.core, method: 'DELETE' })
}

export function getCourierJson(path: string): Promise<unknown> {
  return request(path, { baseUrl: CONFIG.apiBaseUrls.courier, method: 'GET' })
}

export function getCourierJsonWithQuery(path: string, query: URLSearchParams): Promise<unknown> {
  return request(path, { baseUrl: CONFIG.apiBaseUrls.courier, method: 'GET', query })
}

export function postCourierJson(path: string, body: unknown): Promise<unknown> {
  return request(path, { baseUrl: CONFIG.apiBaseUrls.courier, body, method: 'POST' })
}

export function putCourierJson(path: string, body: unknown): Promise<unknown> {
  return request(path, { baseUrl: CONFIG.apiBaseUrls.courier, body, method: 'PUT' })
}

export function deleteCourierJson(path: string): Promise<unknown> {
  return request(path, { baseUrl: CONFIG.apiBaseUrls.courier, method: 'DELETE' })
}

export function getIamJson(path: string): Promise<unknown> {
  return request(path, { baseUrl: CONFIG.apiBaseUrls.iam, method: 'GET' })
}

export function getIamJsonWithQuery(path: string, query: URLSearchParams): Promise<unknown> {
  return request(path, { baseUrl: CONFIG.apiBaseUrls.iam, method: 'GET', query })
}

export function postIamJson(path: string, body: unknown): Promise<unknown> {
  return request(path, { baseUrl: CONFIG.apiBaseUrls.iam, body, method: 'POST' })
}

export function putIamJson(path: string, body: unknown): Promise<unknown> {
  return request(path, { baseUrl: CONFIG.apiBaseUrls.iam, body, method: 'PUT' })
}

export function patchIamJson(path: string, body: unknown): Promise<unknown> {
  return request(path, { baseUrl: CONFIG.apiBaseUrls.iam, body, method: 'PATCH' })
}

export function deleteIamJson(path: string): Promise<unknown> {
  return request(path, { baseUrl: CONFIG.apiBaseUrls.iam, method: 'DELETE' })
}

export function deleteIamJsonWithBody(path: string, body: unknown): Promise<unknown> {
  return request(path, { baseUrl: CONFIG.apiBaseUrls.iam, body, method: 'DELETE' })
}

export function getSentinelJson(path: string): Promise<unknown> {
  return request(path, { baseUrl: CONFIG.apiBaseUrls.sentinel, method: 'GET' })
}

export function getSentinelJsonWithQuery(path: string, query: URLSearchParams): Promise<unknown> {
  return request(path, { baseUrl: CONFIG.apiBaseUrls.sentinel, method: 'GET', query })
}
