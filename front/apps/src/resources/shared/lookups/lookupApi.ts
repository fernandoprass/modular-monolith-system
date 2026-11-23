import { z } from 'zod'
import { API_PATHS } from '../../../data/apiPaths'
import { getJsonWithQuery } from '../../../data/httpClient'
import { unwrapResult } from '../../../data/result'
import type { PagedResultDto } from '../../../shared/pagination'
import { LOOKUP_FILTER_VALUES, type LookupListQuery, type LookupLiteDto } from './lookupTypes'

const lookupPageSchema = z.object({
  items: z.array(z.object({
    id: z.string(), type: z.string(), code: z.string(), value: z.string(),
    description: z.string().nullable(), languageCode: z.string().nullable(),
    isActive: z.boolean(), parentId: z.string().nullable(), organizationId: z.string().nullable(),
  })),
  pageNumber: z.number(), pageSize: z.number(), totalCount: z.number(), totalPages: z.number(),
})

export async function getLookups(request: LookupListQuery): Promise<PagedResultDto<LookupLiteDto>> {
  const query = new URLSearchParams({
    LanguageCode: request.languageCode,
    PageNumber: String(request.pageNumber),
    PageSize: String(request.pageSize),
  })
  if (request.type !== LOOKUP_FILTER_VALUES.all) query.set('Type', request.type)
  if (request.code.trim()) query.set('Code', request.code.trim())
  if (request.value.trim()) query.set('Value', request.value.trim())
  const response = await getJsonWithQuery(API_PATHS.shared.lookups.list, query)
  return lookupPageSchema.parse(unwrapResult<unknown>(response))
}
