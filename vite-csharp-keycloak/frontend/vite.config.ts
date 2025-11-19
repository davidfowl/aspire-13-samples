import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': {
        target: process.env.BFF_HTTPS || process.env.BFF_HTTP || 'http://localhost:5254',
        changeOrigin: true,
        secure: false,
        configure: (proxy) => {
          proxy.on('proxyReq', (proxyReq, req) => {
            // Set forwarded headers so BFF knows the original host
            const host = req.headers.host || 'localhost:9082';
            proxyReq.setHeader('X-Forwarded-Host', host);
            proxyReq.setHeader('X-Forwarded-Proto', 'http');
            proxyReq.setHeader('X-Forwarded-For', req.socket.remoteAddress || '');
          });
        }
      }
    }
  }
})
