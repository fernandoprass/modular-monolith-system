import type {
  DataProvider,
  CreateParams,
  CreateResult,
  DeleteParams,
  DeleteResult,
  GetListParams,
  GetListResult,
  GetOneParams,
  GetOneResult,
  RaRecord,
  UpdateParams,
  UpdateResult,
} from 'react-admin'

import { RESOURCE_NAMES } from '../shared/resourceNames'
import { API_PATHS } from './apiPaths'
import {
  deleteJson,
  ensureResultSuccess,
  getJson,
  getJsonWithQuery,
  putJson,
  unwrapResult,
} from './httpClient'
import type {
  OrganizationCreateForm,
  OrganizationDto,
  PagedResultDto,
} from '../resources/iam/organizations/organizationTypes'
import {
  ORGANIZATION_QUERY_PARAMS,
  ORGANIZATION_REQUEST_FIELDS,
} from '../resources/iam/organizations/organizationTypes'
import { createOrganization as createOrganizationApi } from '../resources/iam/organizations/organizationApi'

type OrganizationRecord = OrganizationDto & RaRecord

function toStringFilter(value: unknown): string | null {
  return typeof value === 'string' && value.trim().length > 0
    ? value.trim()
    : null
}

function toNumberFilter(value: unknown): string | null {
  return typeof value === 'number'
    ? value.toString()
    : null
}

function buildOrganizationQuery(params: GetListParams): URLSearchParams {
  const query = new URLSearchParams()
  const code = toStringFilter(params.filter[ORGANIZATION_QUERY_PARAMS.code])
  const name = toStringFilter(params.filter[ORGANIZATION_QUERY_PARAMS.name])
  const type = toNumberFilter(params.filter[ORGANIZATION_QUERY_PARAMS.type])
  const page = params.pagination?.page ?? 1
  const perPage = params.pagination?.perPage ?? 25

  query.set(ORGANIZATION_QUERY_PARAMS.pageNumber, page.toString())
  query.set(ORGANIZATION_QUERY_PARAMS.pageSize, perPage.toString())

  if (code !== null) {
    query.set(ORGANIZATION_QUERY_PARAMS.code, code)
  }

  if (name !== null) {
    query.set(ORGANIZATION_QUERY_PARAMS.name, name)
  }

  if (type !== null) {
    query.set(ORGANIZATION_QUERY_PARAMS.type, type)
  }

  return query
}

async function getOrganizationList(params: GetListParams): Promise<GetListResult<OrganizationRecord>> {
  const response = await getJsonWithQuery(
    API_PATHS.iam.organizations.list,
    buildOrganizationQuery(params),
  )
  const pagedResult = unwrapResult<PagedResultDto<OrganizationDto>>(response)

  return {
    data: pagedResult.items as OrganizationRecord[],
    total: pagedResult.totalCount,
  }
}

async function getOrganizationOne(params: GetOneParams): Promise<GetOneResult<OrganizationRecord>> {
  const response = await getJson(API_PATHS.iam.organizations.byId(params.id))
  const organization = unwrapResult<OrganizationDto>(response)

  return {
    data: organization as OrganizationRecord,
  }
}

async function updateOrganization(params: UpdateParams): Promise<UpdateResult<OrganizationRecord>> {
  const request = {
    [ORGANIZATION_REQUEST_FIELDS.name]: params.data.name,
    [ORGANIZATION_REQUEST_FIELDS.description]: params.data.description,
    [ORGANIZATION_REQUEST_FIELDS.isActive]: params.data.isActive,
    [ORGANIZATION_REQUEST_FIELDS.defaultLanguage]: params.data.defaultLanguage,
  }
  const response = await putJson(API_PATHS.iam.organizations.byId(params.id), request)

  ensureResultSuccess(response)

  return {
    data: params.data as OrganizationRecord,
  }
}

async function deleteOrganization(params: DeleteParams): Promise<DeleteResult<OrganizationRecord>> {
  const response = await deleteJson(API_PATHS.iam.organizations.byId(params.id))

  ensureResultSuccess(response)

  return {
    data: (params.previousData ?? { id: params.id }) as OrganizationRecord,
  }
}

async function createOrganizationRecord(params: CreateParams): Promise<CreateResult<OrganizationRecord>> {
  const organization = await createOrganizationApi(params.data as OrganizationCreateForm)

  return {
    data: organization as OrganizationRecord,
  }
}

async function unsupportedAction(): Promise<never> {
  throw new Error('shared.notifications.unsupportedDataProviderAction')
}

export const dataProvider: DataProvider = {
  getList: async (resource, params) => {
    if (resource === RESOURCE_NAMES.organizations) {
      return getOrganizationList(params) as Promise<GetListResult>
    }

    return unsupportedAction()
  },
  getOne: async (resource, params) => {
    if (resource === RESOURCE_NAMES.organizations) {
      return getOrganizationOne(params) as Promise<GetOneResult>
    }

    return unsupportedAction()
  },
  getMany: unsupportedAction,
  getManyReference: unsupportedAction,
  create: async (resource, params) => {
    if (resource === RESOURCE_NAMES.organizations) {
      return createOrganizationRecord(params) as Promise<CreateResult>
    }

    return unsupportedAction()
  },
  update: async (resource, params) => {
    if (resource === RESOURCE_NAMES.organizations) {
      return updateOrganization(params) as Promise<UpdateResult>
    }

    return unsupportedAction()
  },
  updateMany: unsupportedAction,
  delete: async (resource, params) => {
    if (resource === RESOURCE_NAMES.organizations) {
      return deleteOrganization(params) as Promise<DeleteResult>
    }

    return unsupportedAction()
  },
  deleteMany: unsupportedAction,
}
