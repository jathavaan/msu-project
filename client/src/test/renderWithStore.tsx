import type { ReactElement, ReactNode } from 'react'
import { configureStore } from '@reduxjs/toolkit'
import { Provider } from 'react-redux'
import { MemoryRouter } from 'react-router-dom'
import { render } from '@testing-library/react'
import { apiSlice } from '../app/apiSlice'

/** Base URL the tests run against, matching `test.env.VITE_API_BASE_URL` in vite.config.ts. */
export const API_URL = 'http://localhost:5205/api'

/**
 * A store built the same way as src/app/store.ts, but a fresh one per test — RTK Query keeps its
 * cache in the store, so reusing the app's singleton would let one test's fetched data satisfy the
 * next test's query and hide a broken request.
 */
function createTestStore() {
  return configureStore({
    reducer: { [apiSlice.reducerPath]: apiSlice.reducer },
    middleware: (getDefaultMiddleware) => getDefaultMiddleware().concat(apiSlice.middleware),
  })
}

/** Renders a component with the Redux/RTK Query and router context its hooks expect. */
export function renderWithStore(ui: ReactElement) {
  function Wrapper({ children }: { children: ReactNode }) {
    return (
      <Provider store={createTestStore()}>
        <MemoryRouter>{children}</MemoryRouter>
      </Provider>
    )
  }

  return render(ui, { wrapper: Wrapper })
}
