import { useState } from 'react'
import type { FormEvent } from 'react'
import { PageHeader } from '../../components/PageHeader'
import { Card } from '../../components/Card'
import { Select } from '../../components/Select'
import { Button } from '../../components/Button'
import { ErrorBanner } from '../../components/ErrorBanner'
import { QueryState } from '../../components/QueryState'
import { getErrorMessage } from '../../lib/apiBaseQuery'
import { formatRecurrenceDay } from '../../lib/formatters'
import { useGetSettingsQuery, useUpdateSettingsMutation } from './api'
import type { Settings } from './types'

const PERIOD_START_DAY_OPTIONS = Array.from({ length: 28 }, (_, i) => i + 1).map((day) => ({
  value: String(day),
  label: formatRecurrenceDay(day),
}))

// Split out from SettingsPage so its period-start-day state can be seeded directly from the
// already-loaded settings in a useState initializer, rather than syncing it in via an effect —
// this only mounts once QueryState has the data, so it only ever initializes once.
function BalancePeriodForm({ settings }: { settings: Settings }) {
  const [updateSettings, { isLoading: isSaving, error: saveError }] = useUpdateSettingsMutation()
  const [periodStartDay, setPeriodStartDay] = useState(String(settings.periodStartDay))
  const [saved, setSaved] = useState(false)
  const saveErrorMessage = getErrorMessage(saveError)

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setSaved(false)
    try {
      await updateSettings({ periodStartDay: Number(periodStartDay) }).unwrap()
      setSaved(true)
    } catch {
      // Surfaced via the mutation's `error` state below.
    }
  }

  return (
    <form onSubmit={handleSubmit} className="flex flex-col gap-4">
      <div className="max-w-xs">
        <Select
          label="Period start day"
          value={periodStartDay}
          onChange={(event) => {
            setPeriodStartDay(event.target.value)
            setSaved(false)
          }}
          options={PERIOD_START_DAY_OPTIONS}
        />
        <p className="mt-2 text-xs text-ink-muted">
          The day your personal month starts — e.g. your payday. The dashboard's "Balance Over the Period" chart
          walks 28 days from this day instead of always starting on the 1st.
        </p>
      </div>
      {saveErrorMessage && <ErrorBanner message={saveErrorMessage} />}
      {saved && <p className="text-sm text-positive">Saved.</p>}
      <div>
        <Button type="submit" disabled={isSaving}>
          Save
        </Button>
      </div>
    </form>
  )
}

export function SettingsPage() {
  const { data: settings, isLoading, error } = useGetSettingsQuery()

  return (
    <>
      <PageHeader title="Settings" description="App-wide preferences for how FinanceOne calculates your dashboard." />

      <Card title="Balance Period">
        <QueryState isLoading={isLoading} error={error}>
          {settings && <BalancePeriodForm settings={settings} />}
        </QueryState>
      </Card>
    </>
  )
}
