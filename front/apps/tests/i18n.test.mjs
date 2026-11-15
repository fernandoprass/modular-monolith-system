import assert from 'node:assert/strict'
import { after, test } from 'node:test'
import { createServer } from 'vite'
import { z } from 'zod'
import { createElement } from 'react'
import { renderToStaticMarkup } from 'react-dom/server'

// Use the app's existing TS/Vite pipeline; no additional test framework is needed.
const server = await createServer({ server: { middlewareMode: true, watch: null, hmr: false, ws: false }, appType: 'custom', optimizeDeps: { noDiscovery: true, include: [] } })
after(() => server.close())
const load = path => server.ssrLoadModule(`/src/${path}`)
const { createTranslate, resolveLocale, LOCALE_DICTIONARIES, UI_LANGUAGE_OPTIONS } = await load('app/i18n/messages.ts')
const { validateDictionary } = await load('app/i18n/dictionaryValidation.ts')
const { LANGUAGE_OPTIONS, ALL_LANGUAGE_OPTIONS, normalizeLanguageCode } = await load('shared/languages.ts')
const { toTranslatedOptions, getLanguageLabel } = await load('shared/translatedOptions.ts')
const { createValidationErrorMap } = await load('shared/validation.ts')
const { formatUserDateTime } = await load('shared/dateFormat.ts')
const t = createTranslate()

test('invalid controls expose their error message to assistive technology', async () => {
  const { Select } = await load('components/ui/select.tsx')
  const { InputSelectTrigger } = await load('components/ui/input-select.tsx')
  const { FieldError } = await load('components/ui/field.tsx')
  const props = { id: 'language', 'aria-invalid': true, 'aria-describedby': 'language-error' }
  const select = renderToStaticMarkup(createElement(Select, { ...props, options: [], onValueChange() {} }))
  const lookup = renderToStaticMarkup(createElement(InputSelectTrigger, {
    ...props, options: [], selectedValue: '', placeholder: 'Select...', setIsPopoverOpen() {},
  }))
  for (const html of [select, lookup]) {
    assert.match(html, /aria-invalid="true"/)
    assert.match(html, /aria-describedby="language-error"/)
    assert.match(html, /id="language"/)
  }
  const error = renderToStaticMarkup(createElement(FieldError, { id: 'language-error' }, t('common.validation.required')))
  assert.match(error, /role="alert"/)
  assert.match(error, /This field is required\./)
  assert.equal(renderToStaticMarkup(createElement(FieldError)), '')
})

test('registered dictionaries have the English keys and placeholders', () => {
  for (const [locale, dictionary] of Object.entries(LOCALE_DICTIONARIES)) {
    assert.deepEqual(validateDictionary(LOCALE_DICTIONARIES['en-US'], dictionary), [], locale)
  }
})

test('dictionary validation rejects wrong placeholders, missing and extra keys, and non-string leaves', () => {
  const source = { page: 'Page {{page}} of {{pages}}', nested: { save: 'Save' } }
  assert.deepEqual(validateDictionary(source, { page: '{{pages}} pages, page {{page}}', nested: { save: 'Salvar' } }), [])
  assert.match(validateDictionary(source, { page: '{{wrong}}', nested: { save: 42 }, extra: 'x' }).join('\n'), /page: interpolation placeholders differ/)
  assert.match(validateDictionary(source, {}).join('\n'), /nested: expected a message object/)
  assert.match(validateDictionary(source, { ...source, extra: 'x' }).join('\n'), /extra: unexpected key/)
  assert.match(validateDictionary(source, { ...source, nested: { save: 42 } }).join('\n'), /nested.save: expected a message string/)
})

test('preference languages match backend support while future French remains defined', () => {
  assert.deepEqual(LANGUAGE_OPTIONS.map(option => option.value), ['en-US', 'pt-BR', 'es-ES'])
  assert.ok(ALL_LANGUAGE_OPTIONS.some(option => option.value === 'fr-FR'))
  assert.deepEqual(UI_LANGUAGE_OPTIONS.map(option => option.value), Object.keys(LOCALE_DICTIONARIES))
  assert.equal(normalizeLanguageCode(' PT-br '), 'pt-BR')
  assert.equal(getLanguageLabel('PT-br', t), 'Portuguese - Brazil')
  assert.equal(getLanguageLabel('unknown', t), 'unknown')
  assert.deepEqual(toTranslatedOptions([{ value: 'save', labelKey: 'common.actions.save' }], t), [{ value: 'save', label: 'Save' }])
})

test('unsupported locales fall back consistently for messages and dates', () => {
  assert.equal(resolveLocale('fr-FR'), 'en-US')
  assert.equal(createTranslate('fr-FR')('common.actions.save'), 'Save')
  assert.equal(formatUserDateTime('2026-01-02T12:34:56Z', 'fr-FR'), formatUserDateTime('2026-01-02T12:34:56Z', 'en-US'))
  assert.equal(formatUserDateTime('not a date', 'en-US'), 'not a date')
})

test('interpolation formats counts and preserves identifier strings', () => {
  assert.equal(t('common.pagination.summary', { page: 1000, pages: 2000 }), 'Page 1,000 of 2,000')
  assert.equal(t('common.pagination.summary', { page: '001', pages: '002' }), 'Page 001 of 002')
})

test('validation uses translated required/email/length messages and retains schema messages', () => {
  const schema = z.object({ name: z.string().min(1), email: z.email(), code: z.string().min(3), custom: z.string().min(1, 'Specific rule') })
  const result = schema.safeParse({ name: '', email: 'invalid', code: 'a', custom: '' }, { error: createValidationErrorMap(t) })
  assert.equal(result.success, false)
  assert.deepEqual(result.error.issues.map(issue => issue.message), [
    'This field is required.', 'Enter a valid email address.', 'Enter at least 3 characters.', 'Specific rule',
  ])
})

test('parameter schemas retain conditional and numeric validation when native validation is disabled', async () => {
  const { createParameterEditSchema } = await load('resources/iam/parameters/parameterValidation.ts')
  const schema = createParameterEditSchema(t)
  const valid = { description: 'Description', externalListEndpoint: '', group: 'Group', isVisible: true,
    listItems: '', module: 'IAM', name: 'Name', overrideType: '0', title: 'Title', type: '8', value: 'Value' }
  assert.equal(schema.safeParse(valid).success, true)
  for (const [changes, field] of [
    [{ value: '' }, 'value'], [{ type: '12' }, 'listItems'], [{ type: '13' }, 'externalListEndpoint'],
    [{ type: '2', value: '1.5' }, 'value'], [{ type: '3', value: 'invalid' }, 'value'],
    [{ type: '7', value: 'AB' }, 'value'],
  ]) {
    const result = schema.safeParse({ ...valid, ...changes }, { error: createValidationErrorMap(t) })
    assert.equal(result.success, false)
    assert.ok(result.error.issues.some(issue => issue.path[0] === field))
  }
})
