import { defaultTheme } from 'react-admin'

export const appTheme = {
  ...defaultTheme,
  palette: {
    ...defaultTheme.palette,
    primary: {
      main: '#2563eb',
    },
    secondary: {
      main: '#0f766e',
    },
    background: {
      default: '#f7f8fa',
      paper: '#ffffff',
    },
  },
  shape: {
    borderRadius: 6,
  },
} as const
