/*
 * File:    main.jsx
 * Purpose: Application entry point. Wraps the app with
 *          React Router for navigation and AuthProvider
 *          for global authentication state.
 * Author:  Abraham Macias
 * Date:    June 2026
 */

import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import { AuthProvider } from './context/AuthContext'
import './index.css'
import App from './App.jsx'

createRoot(document.getElementById('root')).render(
    <StrictMode>
        <BrowserRouter>
            <AuthProvider>
                <App />
            </AuthProvider>
        </BrowserRouter>
    </StrictMode>,
)