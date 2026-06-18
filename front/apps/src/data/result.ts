export type ApiMessage = {
  type: number
  code: string
  text: string
  variables: string[]
}

export type ApiResult<TData> = {
  data: TData | null
  messages: ApiMessage[]
  isSuccess: boolean
  title: string | null
}

export class ApiResultError extends Error {
  messages: ApiMessage[]
  status?: number
  title: string | null

  constructor(title: string | null, messages: ApiMessage[], status?: number) {
    super(title ?? messages.map((message) => message.text).join('\n'))
    this.name = 'ApiResultError'
    this.messages = messages
    this.status = status
    this.title = title
  }

  get notificationText(): string {
    return this.title ?? this.messages.map((message) => message.text).join('\n')
  }
}

export function isApiResult(value: unknown): value is ApiResult<unknown> {
  return typeof value === 'object'
    && value !== null
    && 'messages' in value
    && Array.isArray(value.messages)
    && 'isSuccess' in value
}

export function unwrapResult<TData>(response: unknown): TData {
  if (!isApiResult(response)) {
    throw new ApiResultError(null, [])
  }

  const result = response as ApiResult<TData>

  if (!result.isSuccess || result.data === null) {
    throw new ApiResultError(result.title, result.messages)
  }

  return result.data
}

export function ensureResultSuccess(response: unknown): void {
  if (!isApiResult(response)) {
    throw new ApiResultError(null, [])
  }

  if (!response.isSuccess) {
    throw new ApiResultError(response.title, response.messages)
  }
}

export function getApiErrorText(error: unknown, fallback: string): string {
  if (error instanceof ApiResultError && error.notificationText.length > 0) {
    return error.notificationText
  }

  if (error instanceof Error && error.message.length > 0) {
    return error.message
  }

  return fallback
}
