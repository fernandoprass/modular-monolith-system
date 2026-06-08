import { CONFIG } from '../app/config'
import { tokenStorage } from '../auth/tokenStorage'

type ApiResult<TData> = {
  data: TData | null
  messages: ApiMessage[]
  isSuccess: boolean
  title: string | null
}

type ApiMessage = {
  type: number
  code: string
  text: string
  variables: string[]
}

export class ApiResultError extends Error {
  messages: ApiMessage[]
  title: string | null

  constructor(title: string | null, messages: ApiMessage[]) {
    super(title ?? messages.map((message) => message.text).join('\n'))
    this.name = 'ApiResultError'
    this.messages = messages
    this.title = title
  }

  get notificationText(): string {
    return this.title ?? this.messages.map((message) => message.text).join('\n')
  }
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
  const response = await fetch(`${CONFIG.coreApiUrl}${path}`, {
    method: 'POST',
    body: JSON.stringify(body),
    headers: getHeaders(),
  })

  return readJsonResponse(response)
}

export async function putJson(path: string, body: unknown): Promise<unknown> {
  const response = await fetch(`${CONFIG.coreApiUrl}${path}`, {
    method: 'PUT',
    body: JSON.stringify(body),
    headers: getHeaders(),
  })

  return readJsonResponse(response)
}

export async function patchJson(path: string, body: unknown): Promise<unknown> {
  const response = await fetch(`${CONFIG.coreApiUrl}${path}`, {
    method: 'PATCH',
    body: JSON.stringify(body),
    headers: getHeaders(),
  })

  return readJsonResponse(response)
}

export async function deleteJson(path: string): Promise<unknown> {
  const response = await fetch(`${CONFIG.coreApiUrl}${path}`, {
    method: 'DELETE',
    headers: getHeaders(),
  })

  return readJsonResponse(response)
}

export async function getJson(path: string): Promise<unknown> {
  const response = await fetch(`${CONFIG.coreApiUrl}${path}`, {
    method: 'GET',
    headers: getHeaders(),
  })

  return readJsonResponse(response)
}

async function readJsonResponse(response: Response): Promise<unknown> {
  const result = await readResponseBody(response)
  ensureHttpSuccess(response, result)
  return result
}

async function readResponseBody(response: Response): Promise<unknown> {
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

    throw new Error(response.statusText)
  }
}

export async function getJsonWithQuery(path: string, query: URLSearchParams): Promise<unknown> {
  const queryString = query.toString()
  const url = queryString.length > 0
    ? `${CONFIG.coreApiUrl}${path}?${queryString}`
    : `${CONFIG.coreApiUrl}${path}`

  const response = await fetch(url, {
    method: 'GET',
    headers: getHeaders(),
  })

  return readJsonResponse(response)
}

function ensureHttpSuccess(response: Response, result: unknown): void {
  if (response.ok) {
    return
  }

  if (isApiResult(result)) {
    throw new ApiResultError(result.title, result.messages)
  }

  throw new Error(response.statusText)
}

function isApiResult(value: unknown): value is ApiResult<unknown> {
  return typeof value === 'object'
    && value !== null
    && 'messages' in value
    && Array.isArray(value.messages)
}

export function unwrapResult<TData>(response: unknown): TData {
  if (!isApiResult(response)) {
    throw new Error('Invalid API result.')
  }

  const result = response as ApiResult<TData>

  if (!result.isSuccess) {
    throw new ApiResultError(result.title, result.messages)
  }

  if (result.data === null) {
    throw new ApiResultError(result.title, result.messages)
  }

  return result.data
}

export function ensureResultSuccess(response: unknown): void {
  if (!isApiResult(response)) {
    throw new Error('Invalid API result.')
  }

  const result = response as ApiResult<unknown>

  if (!result.isSuccess) {
    throw new ApiResultError(result.title, result.messages)
  }
}

export function getApiErrorText(error: unknown): string {
  if (error instanceof ApiResultError) {
    return error.notificationText
  }

  if (error instanceof Error) {
    return error.message
  }

  return ''
}
