import { Layout, type LayoutProps } from 'react-admin'

import { AppMenu } from './AppMenu'

export function AppLayout(props: LayoutProps) {
  return <Layout {...props} menu={AppMenu} />
}
