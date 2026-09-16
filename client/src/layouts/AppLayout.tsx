import { useState } from 'react'
import { Outlet } from 'react-router-dom'
import { Menu, CircleDollarSign } from 'lucide-react'
import { Sidebar } from './Sidebar'

export function AppLayout() {
  const [sidebarOpen, setSidebarOpen] = useState(false)

  return (
    <div className="flex min-h-screen bg-page">
      <Sidebar open={sidebarOpen} onClose={() => setSidebarOpen(false)} />

      {sidebarOpen && (
        <button
          aria-label="Close menu overlay"
          onClick={() => setSidebarOpen(false)}
          className="fixed inset-0 z-40 bg-slate-900/40 md:hidden"
        />
      )}

      <div className="flex min-w-0 flex-1 flex-col">
        <header className="sticky top-0 z-30 flex items-center gap-3 border-b border-border bg-surface px-4 py-3 md:hidden">
          <button
            aria-label="Open menu"
            onClick={() => setSidebarOpen(true)}
            className="rounded-lg p-1.5 text-ink hover:bg-page"
          >
            <Menu size={22} />
          </button>
          <div className="flex items-center gap-2 text-ink">
            <CircleDollarSign size={20} className="text-positive" />
            <span className="text-sm font-semibold">FinanceOne</span>
          </div>
        </header>

        <main className="min-w-0 flex-1 p-4 md:p-8">
          <div className="mx-auto max-w-6xl">
            <Outlet />
          </div>
        </main>
      </div>
    </div>
  )
}
