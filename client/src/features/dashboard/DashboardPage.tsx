import { PiggyBank, Scale, TrendingDown, TrendingUp } from 'lucide-react'
import { PageHeader } from '../../components/PageHeader'
import { Card } from '../../components/Card'
import { StatTile } from './StatTile'
import { SpendingByCategoryChart } from './SpendingByCategoryChart'
import { IncomeVsExpensesChart } from './IncomeVsExpensesChart'
import { BalanceOverTimeChart } from './BalanceOverTimeChart'
import { UpcomingPaymentsWidget } from './UpcomingPaymentsWidget'
import { BudgetSnapshot } from './BudgetSnapshot'
import { SavingGoalsSnapshot } from './SavingGoalsSnapshot'
import { MonthlySavingsSnapshot } from './MonthlySavingsSnapshot'
import { useGetIncomesQuery } from '../income/api'
import { useGetExpensesQuery } from '../expenses/api'
import { useGetSavingGoalsQuery } from '../saving-goals/api'
import { useGetMonthlySavingsQuery } from '../monthly-savings/api'
import { formatCurrency } from '../../lib/formatters'

export function DashboardPage() {
  const { data: incomes } = useGetIncomesQuery()
  const { data: expenses } = useGetExpensesQuery({})
  const { data: savingGoals } = useGetSavingGoalsQuery()
  const { data: monthlySavings } = useGetMonthlySavingsQuery()

  const totalIncome = incomes?.reduce((sum, income) => sum + income.amount, 0) ?? 0
  const totalExpenses = expenses?.reduce((sum, expense) => sum + expense.amount, 0) ?? 0
  const totalMonthlySavings = monthlySavings?.reduce((sum, saving) => sum + saving.amount, 0) ?? 0
  // What's left once both recurring expenses and what you've committed to saving this month
  // are accounted for — distinct from `net`, which ignores savings entirely.
  const availableAfterSavings = totalIncome - totalExpenses - totalMonthlySavings
  const totalSaved = savingGoals?.reduce((sum, goal) => sum + goal.amountSaved, 0) ?? 0

  return (
    <>
      <PageHeader title="Dashboard" description="Your monthly recurring income and expenses at a glance." />

      <div className="mb-6 grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <StatTile label="Monthly Income" value={formatCurrency(totalIncome)} icon={<TrendingUp size={18} />} tone="positive" />
        <StatTile label="Monthly Expenses" value={formatCurrency(totalExpenses)} icon={<TrendingDown size={18} />} tone="negative" />
        <StatTile
          label="Available After Savings"
          value={formatCurrency(availableAfterSavings)}
          icon={<Scale size={18} />}
          tone={availableAfterSavings >= 0 ? 'positive' : 'negative'}
        />
        <StatTile label="Total Saved" value={formatCurrency(totalSaved)} icon={<PiggyBank size={18} />} />
      </div>

      <div className="mb-6">
        <Card title="Balance Over the Period" actions={<span className="text-xs text-ink-muted">Hover a day for details</span>}>
          <BalanceOverTimeChart />
        </Card>
      </div>

      <div className="mb-6 grid grid-cols-1 gap-4 lg:grid-cols-2">
        <Card title="Spending by Category">
          <SpendingByCategoryChart expenses={expenses ?? []} />
        </Card>
        <Card title="Income vs Expenses vs Savings">
          <IncomeVsExpensesChart totalIncome={totalIncome} totalExpenses={totalExpenses} totalMonthlySavings={totalMonthlySavings} />
        </Card>
      </div>

      <div className="grid grid-cols-1 gap-4 lg:grid-cols-2 xl:grid-cols-4">
        <UpcomingPaymentsWidget />
        <BudgetSnapshot />
        <SavingGoalsSnapshot />
        <MonthlySavingsSnapshot />
      </div>
    </>
  )
}
