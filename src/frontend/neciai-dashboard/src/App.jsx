/*
 * File:    App.jsx
 * Purpose: Root application component. Defines all routes
 *          and protects dashboard routes from unauthenticated
 *          access by redirecting to the login page.
 * Author:  Abraham Macias
 * Date:    June 2026
 */

import { Routes, Route, Navigate } from 'react-router-dom'
import { useAuth } from './context/AuthContext'
import LoginPage from './pages/LoginPage'
import DashboardPage from './pages/DashboardPage'
import RecordsPage from './pages/RecordsPage'
import ReportsPage from './pages/ReportsPage'
import SearchPage from './pages/SearchPage'
import Layout from './components/Layout'

/**
 * ProtectedRoute — redirects unauthenticated users to login.
 * Wraps any route that requires authentication.
 */
function ProtectedRoute({ children }) {
    const { isAuthenticated } = useAuth()
    return isAuthenticated ? children : <Navigate to="/login" replace />
}

export default function App() {
    return (
        <Routes>
            {/* Public routes */}
            <Route path="/login" element={<LoginPage />} />

            {/* Protected routes — all wrapped in Layout */}
            <Route path="/" element={
                <ProtectedRoute>
                    <Layout />
                </ProtectedRoute>
            }>
                <Route index element={<Navigate to="/dashboard" replace />} />
                <Route path="dashboard" element={<DashboardPage />} />
                <Route path="records" element={<RecordsPage />} />
                <Route path="reports" element={<ReportsPage />} />
                <Route path="search" element={<SearchPage />} />
            </Route>

            {/* Catch all — redirect to dashboard */}
            <Route path="*" element={<Navigate to="/dashboard" replace />} />
        </Routes>
    )
}