export const CONFIG = {
  apiBaseUrls: {
    core: import.meta.env.VITE_CORE_API_URL ?? 'http://localhost:5050',
    courier: import.meta.env.VITE_COURIER_API_URL ?? import.meta.env.VITE_CORE_API_URL ?? 'http://localhost:5050',
    iam: import.meta.env.VITE_IAM_API_URL ?? import.meta.env.VITE_CORE_API_URL ?? 'http://localhost:5050',
    sentinel: import.meta.env.VITE_SENTINEL_API_URL ?? import.meta.env.VITE_CORE_API_URL ?? 'http://localhost:5050',
  },
} as const
