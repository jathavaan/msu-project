// defineConfig comes from vitest/config rather than vite so the `test` block below is typed.
// It is the same function, re-exported.
import { defineConfig } from 'vitest/config'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), tailwindcss()],
  test: {
    // Components render against the DOM, so jsdom rather than the default node environment.
    environment: 'jsdom',
    globals: true,
    setupFiles: ['./src/test/setup.ts'],
    // Vite bakes this in at build time; tests need a stable base URL for MSW to intercept.
    env: { VITE_API_BASE_URL: 'http://localhost:5205' },
    css: false,
    // CI additionally writes JUnit XML for dorny/test-reporter to turn into a check run; locally
    // the plain reporter is all that is wanted. GitHub Actions sets CI=true.
    reporters: process.env.CI ? ['default', 'junit'] : ['default'],
    outputFile: { junit: './test-results/junit.xml' },
    coverage: {
      provider: 'v8',
      reporter: ['text', 'lcov'],
      include: ['src/**/*.{ts,tsx}'],
      exclude: ['src/test/**', 'src/**/*.test.{ts,tsx}', 'src/main.tsx', 'src/vite-env.d.ts'],
    },
  },
})
