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
  postJson,
  putJson,
  unwrapResult,
} from './httpClient'
import type {
  OrganizationCreateForm,
  OrganizationCreateRequest,
  OrganizationDto,
  PagedResultDto,
} from '../resources/iam/organizations/organizationTypes'

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
  const code = toStringFilter(params.filter.Code)
  const name = toStringFilter(params.filter.Name)
  const type = toNumberFilter(params.filter.Type)
  const page = params.pagination?.page ?? 1
  const perPage = params.pagination?.perPage ?? 25

  query.set('PageNumber', page.toString())
  query.set('PageSize', perPage.toString())

  if (code !== null) {
    query.set('Code', code)
  }

  if (name !== null) {
    query.set('Name', name)
  }

  if (type !== null) {
    query.set('Type', type)
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
    Name: params.data.name,
    Description: params.data.description,
    IsActive: params.data.isActive,
    DefaultLanguage: params.data.defaultLanguage,
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

function toOrganizationCreateRequest(data: OrganizationCreateForm): OrganizationCreateRequest {
  return {
    Type: data.type,
    Name: data.name,
    Code: data.code,
    Description: data.description,
    DefaultLanguage: data.defaultLanguage,
    User: {
      Name: data.userName,
      Email: data.userEmail,
      Password: data.userPassword,
    },
  }
}

async function createOrganization(params: CreateParams): Promise<CreateResult<OrganizationRecord>> {
  const response = await postJson(
    API_PATHS.iam.organizations.list,
    toOrganizationCreateRequest(params.data as OrganizationCreateForm),
  )
  const organization = unwrapResult<OrganizationDto>(response)

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
      return createOrganization(params) as Promise<CreateResult>
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
