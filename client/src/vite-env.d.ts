/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_API_BASE_URL: string
  /** Optional — unset in local dev, so nothing tries to phone home. See src/lib/appInsights.ts. */
  readonly VITE_APPLICATIONINSIGHTS_CONNECTION_STRING?: string
}
