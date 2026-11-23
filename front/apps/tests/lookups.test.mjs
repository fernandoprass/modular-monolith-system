import assert from 'node:assert/strict'
import { after, beforeEach, test } from 'node:test'
import { createServer } from 'vite'
import { createElement } from 'react'
import { renderToStaticMarkup } from 'react-dom/server'
import { MemoryRouter } from 'react-router-dom'

const server = await createServer({ server: { middlewareMode: true, watch: null, hmr: false, ws: false }, appType: 'custom', optimizeDeps: { noDiscovery: true, include: [] } })
const originalStorage = Object.getOwnPropertyDescriptor(globalThis, 'localStorage')
const originalFetch = globalThis.fetch
after(async () => {
  await server.close()
  if (originalStorage) Object.defineProperty(globalThis, 'localStorage', originalStorage)
  else delete globalThis.localStorage
  globalThis.fetch = originalFetch
})
const { tokenStorage } = await server.ssrLoadModule('/src/auth/tokenStorage.ts')
const { getLookups } = await server.ssrLoadModule('/src/resources/shared/lookups/lookupApi.ts')
const { LookupListPage } = await server.ssrLoadModule('/src/resources/shared/lookups/LookupListPage.tsx')
const { AuthProvider } = await server.ssrLoadModule('/src/auth/AuthProvider.tsx')
const { ToastProvider } = await server.ssrLoadModule('/src/app/ToastProvider.tsx')

beforeEach(() => {
  const items = new Map()
  Object.defineProperty(globalThis, 'localStorage', { configurable: true, value: {
    getItem: key => items.get(key) ?? null,
    setItem: (key, value) => items.set(key, value),
    removeItem: key => items.delete(key),
  } })
})

test('lookup API sends server filters and pagination and unwraps empty pages', async () => {
  const page = { items: [], pageNumber: 2, pageSize: 10, totalCount: 0, totalPages: 0 }
  let requested
  globalThis.fetch = async url => {
    requested = new URL(url)
    return new Response(JSON.stringify({ data: page, isSuccess: true, messages: [] }), { status: 200 })
  }
  const result = await getLookups({ code: ' BR ', value: ' Brasil ', type: 'state', languageCode: 'pt-BR', pageNumber: 2, pageSize: 10 })
  assert.equal(requested.pathname, '/api/v1/shared/lookups')
  assert.deepEqual(Object.fromEntries(requested.searchParams), { LanguageCode: 'pt-BR', PageNumber: '2', PageSize: '10', Type: 'state', Code: 'BR', Value: 'Brasil' })
  assert.deepEqual(result, page)
  await getLookups({ code: ' ', value: '', type: 'all', languageCode: 'en-US', pageNumber: 1, pageSize: 25 })
  assert.equal(requested.searchParams.has('Type'), false)
  assert.equal(requested.searchParams.has('Code'), false)
  assert.equal(requested.searchParams.has('Value'), false)
})

function renderSearch(isSystemAdmin, permissions) {
  tokenStorage.setToken('test-token')
  tokenStorage.setUser({ id: 'test-user', fullName: 'Test', language: 'pt-BR', isSystemAdmin })
  tokenStorage.setPermissions(permissions)
  return renderToStaticMarkup(createElement(AuthProvider, null,
    createElement(ToastProvider, null, createElement(MemoryRouter, null, createElement(LookupListPage)))))
}

test('only sysadmins see Type; readers retain language, Code and Value filters', () => {
  const admin = renderSearch(true, [])
  assert.match(admin, /id="lookup-type"/)
  const reader = renderSearch(false, ['shared.lookups.read'])
  assert.doesNotMatch(reader, /id="lookup-type"/)
  assert.match(reader, /id="lookup-language"/)
  assert.match(reader, /Portuguese/)
  assert.match(reader, /id="lookup-code"/)
  assert.match(reader, /id="lookup-value"/)
  assert.doesNotMatch(renderSearch(false, []), /id="lookup-code"/)
})
