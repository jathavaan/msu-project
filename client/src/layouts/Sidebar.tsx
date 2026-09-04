import { NavLink } from 'react-router-dom'
import {
  LayoutDashboard,
  TrendingUp,
  TrendingDown,
  Wallet,
  Tags,
  Target,
  PiggyBank,
  Ticket,
  CalendarClock,
  CircleDollarSign,
} from 'lucide-react'
import clsx from 'clsx'

const NAV_ITEMS = [
  { to: '/', label: 'Dashboard', icon: LayoutDashboard, end: true },
  { to: '/income', label: 'Income', icon: TrendingUp },
  { to: '/expenses', label: 'Expenses', icon: TrendingDown },
  { to: '/budgets', label: 'Budgets', icon: Wallet },
  { to: '/saving-goals', label: 'Saving Goals', icon: Target },
  { to: '/monthly-savings', label: 'Monthly Savings', icon: PiggyBank },
  { to: '/upcoming-payments', label: 'Upcoming Payments', icon: CalendarClock },
  { to: '/discount-codes', label: 'Discount Codes', icon: Ticket },
  { to: '/categories', label: 'Categories', icon: Tags },
]

export function Sidebar() {
  return (
    <aside className="sticky top-0 flex h-screen w-64 shrink-0 self-start flex-col bg-sidebar px-3 py-5">
      <div className="mb-6 flex items-center gap-2 px-2 text-white">
        <CircleDollarSign size={22} className="text-sidebar-active" />
        <span className="text-base font-semibold">FinanceOne</span>
      </div>

      <nav className="flex flex-1 flex-col gap-1">
        {NAV_ITEMS.map(({ to, label, icon: Icon, end }) => (
          <NavLink
            key={to}
            to={to}
            end={end}
            className={({ isActive }) =>
              clsx(
                'flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors',
                isActive ? 'bg-sidebar-hover text-sidebar-text-active' : 'text-sidebar-text hover:bg-sidebar-hover hover:text-sidebar-text-active',
              )
            }
          >
            <Icon size={18} />
            {label}
          </NavLink>
        ))}
      </nav>
    </aside>
  )
}
