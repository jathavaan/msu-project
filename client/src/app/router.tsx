import { createBrowserRouter } from 'react-router-dom'
import { AppLayout } from '../layouts/AppLayout'
import { DashboardPage } from '../features/dashboard/DashboardPage'
import { IncomePage } from '../features/income/IncomePage'
import { ExpensesPage } from '../features/expenses/ExpensesPage'
import { BudgetsPage } from '../features/budgets/BudgetsPage'
import { CategoriesPage } from '../features/categories/CategoriesPage'
import { SavingGoalsPage } from '../features/saving-goals/SavingGoalsPage'
import { MonthlySavingsPage } from '../features/monthly-savings/MonthlySavingsPage'
import { DiscountCodesPage } from '../features/discount-codes/DiscountCodesPage'
import { UpcomingPaymentsPage } from '../features/upcoming-payments/UpcomingPaymentsPage'

export const router = createBrowserRouter([
  {
    path: '/',
    element: <AppLayout />,
    children: [
      { index: true, element: <DashboardPage /> },
      { path: 'income', element: <IncomePage /> },
      { path: 'expenses', element: <ExpensesPage /> },
      { path: 'budgets', element: <BudgetsPage /> },
      { path: 'saving-goals', element: <SavingGoalsPage /> },
      { path: 'monthly-savings', element: <MonthlySavingsPage /> },
      { path: 'discount-codes', element: <DiscountCodesPage /> },
      { path: 'upcoming-payments', element: <UpcomingPaymentsPage /> },
      { path: 'categories', element: <CategoriesPage /> },
    ],
  },
])
