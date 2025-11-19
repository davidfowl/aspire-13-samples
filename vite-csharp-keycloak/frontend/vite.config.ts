import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': {
        target: process.env.BFF_HTTPS || process.env.BFF_HTTP,
        changeOrigin: true,
        secure: false,
        xfwd: true, // Send X-Forwarded-* headers to BFF
      }
    }
  }
})
