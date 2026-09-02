const currencyFormatter = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' })

export function formatCurrency(amount: number): string {
  return currencyFormatter.format(amount)
}

const dateFormatter = new Intl.DateTimeFormat('en-US', { month: 'short', day: 'numeric', year: 'numeric' })

/** `isoDate` is a `DateOnly` from the API, formatted as `YYYY-MM-DD`. */
export function formatDate(isoDate: string): string {
  return dateFormatter.format(new Date(`${isoDate}T00:00:00`))
}

export function formatRecurrenceDay(day: number): string {
  const remainder = day % 100
  const suffix =
    remainder >= 11 && remainder <= 13
      ? 'th'
      : (['th', 'st', 'nd', 'rd'][day % 10] ?? 'th')
  return `${day}${suffix} of the month`
}

export function daysUntil(isoDate: string): number {
  const target = new Date(`${isoDate}T00:00:00`)
  const today = new Date()
  today.setHours(0, 0, 0, 0)
  return Math.round((target.getTime() - today.getTime()) / (1000 * 60 * 60 * 24))
}
