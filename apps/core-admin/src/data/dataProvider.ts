import type { DataProvider } from 'react-admin'

async function unsupportedAction(): Promise<never> {
  throw new Error('shared.notifications.unsupportedDataProviderAction')
}

export const dataProvider: DataProvider = {
  getList: unsupportedAction,
  getOne: unsupportedAction,
  getMany: unsupportedAction,
  getManyReference: unsupportedAction,
  create: unsupportedAction,
  update: unsupportedAction,
  updateMany: unsupportedAction,
  delete: unsupportedAction,
  deleteMany: unsupportedAction,
}
