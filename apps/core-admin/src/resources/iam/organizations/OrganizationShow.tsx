import { BooleanField, SelectField, Show, SimpleShowLayout, TextField } from 'react-admin'

import { languageChoices } from '../../../shared/languages'
import { organizationTypeChoices } from './organizationTypes'

export function OrganizationShow() {
  return (
    <Show>
      <SimpleShowLayout>
        <SelectField choices={organizationTypeChoices} source="type" />
        <TextField source="code" />
        <TextField source="name" />
        <TextField source="description" />
        <SelectField choices={languageChoices} source="defaultLanguage" />
        <BooleanField source="isActive" />
      </SimpleShowLayout>
    </Show>
  )
}
