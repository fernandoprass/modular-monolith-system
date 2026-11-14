type MessageTree = { [key: string]: string | MessageTree }

function placeholders(message: string): string {
  return [...new Set(Array.from(message.matchAll(/\{\{([^{}]+)\}\}/g), match => match[1]))].sort().join(',')
}

/** Checks keys, leaf types and placeholder names, allowing translators to reorder variables. */
export function validateDictionary(reference: MessageTree, dictionary: unknown, path = ''): string[] {
  if (typeof dictionary !== 'object' || dictionary === null || Array.isArray(dictionary)) {
    return [`${path || '<root>'}: expected a message object`]
  }

  const errors: string[] = []
  const candidate = dictionary as Record<string, unknown>
  for (const [key, expected] of Object.entries(reference)) {
    const keyPath = path ? `${path}.${key}` : key
    const actual = Object.hasOwn(candidate, key) ? candidate[key] : undefined
    if (typeof expected === 'string') {
      if (typeof actual !== 'string') errors.push(`${keyPath}: expected a message string`)
      else if (placeholders(actual) !== placeholders(expected)) errors.push(`${keyPath}: interpolation placeholders differ`)
    } else {
      errors.push(...validateDictionary(expected, actual, keyPath))
    }
  }
  for (const key of Object.keys(candidate)) {
    if (!Object.hasOwn(reference, key)) errors.push(`${path ? `${path}.` : ''}${key}: unexpected key`)
  }
  return errors
}
