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

  const result = await response.json() as unknown

  if (!response.ok) {
    const apiResult = result as ApiResult<unknown>

    if (Array.isArray(apiResult.messages)) {
      throw new ApiResultError(apiResult.title, apiResult.messages)
    }

    throw new Error(response.statusText)
  }

  return result
}

export function unwrapResult<TData>(response: unknown): TData {
  const result = response as ApiResult<TData>

  if (!result.isSuccess) {
    throw new ApiResultError(result.title, result.messages)
  }

  if (result.data === null) {
    throw new ApiResultError(result.title, result.messages)
  }

  return result.data
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
