import { afterEach, describe, expect, it, vi } from 'vitest'

const loadAppInsights = vi.fn()

vi.mock('@microsoft/applicationinsights-web', () => ({
  ApplicationInsights: vi.fn().mockImplementation(function (this: object, options: unknown) {
    Object.assign(this, { config: options, loadAppInsights })
  }),
}))

describe('appInsights', () => {
  afterEach(() => {
    vi.unstubAllEnvs()
    vi.resetModules()
    loadAppInsights.mockClear()
  })

  it('stays null and never loads when no connection string is configured', async () => {
    vi.stubEnv('VITE_APPLICATIONINSIGHTS_CONNECTION_STRING', '')

    const { appInsights } = await import('./appInsights')

    expect(appInsights).toBeNull()
    expect(loadAppInsights).not.toHaveBeenCalled()
  })

  it('initializes with auto route tracking when a connection string is configured', async () => {
    vi.stubEnv('VITE_APPLICATIONINSIGHTS_CONNECTION_STRING', 'InstrumentationKey=abc-123')

    const { appInsights } = await import('./appInsights')

    expect(appInsights).not.toBeNull()
    expect(loadAppInsights).toHaveBeenCalledOnce()
  })
})
