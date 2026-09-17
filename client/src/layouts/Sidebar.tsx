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
  X,
} from 'lucide-react'
import clsx from 'clsx'

// Spend Trends (issue #49) is disabled for now — its page/nav entry are deliberately left out of
// this list rather than deleted; see the SpendTrendsPage import commented out in router.tsx.
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

interface SidebarProps {
  open: boolean
  onClose: () => void
}

export function Sidebar({ open, onClose }: SidebarProps) {
  return (
    <aside
      className={clsx(
        'fixed inset-y-0 left-0 z-50 flex h-screen w-64 shrink-0 flex-col bg-sidebar px-3 py-5 transition-transform duration-200 ease-in-out',
        'md:sticky md:top-0 md:z-auto md:translate-x-0 md:self-start',
        open ? 'translate-x-0' : '-translate-x-full',
      )}
    >
      <div className="mb-6 flex items-center justify-between px-2 text-white">
        <div className="flex items-center gap-2">
          <CircleDollarSign size={22} className="text-sidebar-active" />
          <span className="text-base font-semibold">FinanceOne</span>
        </div>
        <button
          aria-label="Close menu"
          onClick={onClose}
          className="rounded-full p-1 text-sidebar-text hover:bg-sidebar-hover hover:text-sidebar-text-active md:hidden"
        >
          <X size={20} />
        </button>
      </div>

      <nav className="flex flex-1 flex-col gap-1 overflow-y-auto">
        {NAV_ITEMS.map(({ to, label, icon: Icon, end }) => (
          <NavLink
            key={to}
            to={to}
            end={end}
            onClick={onClose}
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
