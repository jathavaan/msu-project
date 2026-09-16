import { ApplicationInsights } from '@microsoft/applicationinsights-web'

const connectionString = import.meta.env.VITE_APPLICATIONINSIGHTS_CONNECTION_STRING

/**
 * `null` whenever no connection string is baked in (local dev, docker-compose) — nothing here
 * tries to reach Azure unless a real value was supplied at build time. `enableAutoRouteTracking`
 * covers page views for this SPA's client-side routing (no history object/React plugin needed),
 * and the SDK's default `disableExceptionTracking: false` covers unhandled client-side errors.
 */
export const appInsights = connectionString
  ? new ApplicationInsights({
      config: {
        connectionString,
        enableAutoRouteTracking: true,
        autoTrackPageVisitTime: true,
      },
    })
  : null

appInsights?.loadAppInsights()
