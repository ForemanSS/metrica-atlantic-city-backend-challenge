import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],

  server: {
    port: 5173,
    strictPort: true,

    proxy: {
      '/auth': {
        target: 'https://localhost:7279',
        changeOrigin: true,
        secure: false,
      },

      '/api': {
        target: 'https://localhost:7279',
        changeOrigin: true,
        secure: false,
      },
    },
  },
})