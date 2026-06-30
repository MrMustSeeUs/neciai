// vite.config.js
// Purpose: Vite configuration for NeciAI React PWA
// Adds Tailwind CSS plugin and sets the API proxy so
// frontend calls to /api automatically route to the
// C# backend running on port 5005 during development.

import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

export default defineConfig({
    plugins: [
        react(),
        tailwindcss(),
    ],
    server: {
        proxy: {
            '/api': {
                target: 'http://localhost:5005',
                changeOrigin: true,
                secure: false,
            }
        }
    }
})