import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      // проксируем API на бэкенд в dev-режиме
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true,
      },
      // analytics-сервис (FastAPI) в dev. Префикс /api/analytics, чтобы
      // не перехватывать SPA-роут /analytics (та же причина, что в nginx.conf).
      '/api/analytics': {
        target: 'http://localhost:8000',
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/api\/analytics/, ''),
      },
    },
  },
})
