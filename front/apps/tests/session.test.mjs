import assert from 'node:assert/strict'
import { after, beforeEach, test } from 'node:test'
import { createServer } from 'vite'
import { createElement } from 'react'
import { renderToStaticMarkup } from 'react-dom/server'

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
const { getIamJson } = await server.ssrLoadModule('/src/data/httpClient.ts')
const { AuthProvider, useAuth } = await server.ssrLoadModule('/src/auth/AuthProvider.tsx')

// Capture the real provider callbacks without introducing a browser test dependency.
function captureAuth() {
  let auth
  function Capture() {
    auth = useAuth()
    return null
  }
  renderToStaticMarkup(createElement(AuthProvider, null, createElement(Capture)))
  return auth
}
beforeEach(() => {
  const items = new Map()
  Object.defineProperty(globalThis, 'localStorage', { configurable: true, value: {
    getItem: key => items.get(key) ?? null,
    setItem: (key, value) => items.set(key, value),
    removeItem: key => items.delete(key),
  } })
  tokenStorage.clearAll()
})
function signIn(id, language = 'en-US') {
  tokenStorage.setToken(`token-${id}`)
  tokenStorage.setUser({ id, fullName: id, language, email: `${id}@example.com`, organizationId: id, organizationName: id, isOrganizationAdmin: false, isSystemAdmin: false })
  tokenStorage.setPermissions([`${id}.permission`])
  return tokenStorage.getSession()
}

test('a delayed profile response cannot replace a new user or revive a logged-out session', () => {
  const sessionA = signIn('A')
  tokenStorage.clearAll()
  assert.equal(tokenStorage.updateProfile('A', { fullName: 'Old A', language: 'pt-BR' }, sessionA), null)
  signIn('B')
  assert.equal(tokenStorage.updateProfile('A', { fullName: 'Old A', language: 'pt-BR' }, sessionA), null)
  assert.equal(tokenStorage.getUser().id, 'B')
  assert.equal(tokenStorage.getToken(), 'token-B')
  assert.deepEqual(tokenStorage.getPermissions(), ['B.permission'])
})

test('a new login with the same token still invalidates the previous session', () => {
  const previous = signIn('A')
  signIn('A')
  assert.equal(tokenStorage.updateProfile('A', { fullName: 'Stale', language: 'pt-BR' }, previous), null)
})

test('saved language applies to the next request without another login', async () => {
  const session = signIn('A')
  assert.equal(tokenStorage.updateProfile('A', { fullName: 'Updated', language: 'pt-br' }, session).language, 'pt-BR')
  globalThis.fetch = async (_url, options) => {
    assert.equal(options.headers.get('Accept-Language'), 'pt-BR')
    assert.equal(options.headers.get('Authorization'), 'Bearer token-A')
    return new Response('{}')
  }
  await getIamJson('/test')
})

test('an old unauthorized response cannot clear a new session', async () => {
  signIn('A')
  let respond
  globalThis.fetch = () => new Promise(resolve => { respond = resolve })
  const request = getIamJson('/test')
  signIn('B')
  respond(new Response('', { status: 401 }))
  await assert.rejects(request)
  assert.equal(tokenStorage.getUser().id, 'B')
})

test('forbidden responses retain authentication; current unauthorized responses clear it', async () => {
  signIn('A')
  globalThis.fetch = async () => new Response('', { status: 403 })
  await assert.rejects(getIamJson('/test'))
  assert.equal(tokenStorage.getUser().id, 'A')
  globalThis.fetch = async () => new Response('', { status: 401 })
  await assert.rejects(getIamJson('/test'))
  assert.equal(tokenStorage.getUser(), null)
})

test('the original provider callback rejects a profile save after switching users', () => {
  const sessionA = signIn('A')
  const auth = captureAuth()
  auth.logout()
  signIn('B')
  assert.equal(auth.updateProfile('A', { fullName: 'Stale A', language: 'pt-BR' }, sessionA), false)
  assert.equal(tokenStorage.getUser().id, 'B')
})

test('login failures are propagated to the form rather than reported as success', async () => {
  const auth = captureAuth()
  globalThis.fetch = async () => new Response('', { status: 401 })
  await assert.rejects(auth.login({ email: 'A@example.com', password: 'invalid' }))
  assert.equal(tokenStorage.getToken(), null)
})

test('logout while login is pending prevents the response from restoring authentication', async () => {
  const auth = captureAuth()
  let respond
  globalThis.fetch = () => new Promise(resolve => { respond = resolve })
  const pendingLogin = auth.login({ email: 'A@example.com', password: 'password' })
  auth.logout()
  respond(Response.json({ isSuccess: true, messages: [], data: {
    token: 'late-token', user: { id: 'A', name: 'A', language: 'en-US' },
  } }))
  await pendingLogin
  assert.equal(tokenStorage.getUser(), null)
  assert.equal(tokenStorage.getToken(), null)
})
