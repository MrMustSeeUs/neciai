/*
 * File:    AuthContext.jsx
 * Purpose: Global authentication context for NeciAI.
 *          Stores the current user and JWT token in memory
 *          and localStorage. Provides login, logout, and
 *          user state to every component in the app.
 * Author:  Abraham Macias
 * Date:    June 2026
 */

import { createContext, useContext, useState, useEffect } from 'react'
import { login as apiLogin, register as apiRegister } from '../services/api'

// Create the context — this is what components will consume
const AuthContext = createContext(null)

/**
 * AuthProvider wraps the entire app and makes auth state
 * available to any component without prop drilling.
 */
export function AuthProvider({ children }) {
    const [user, setUser] = useState(null)
    const [token, setToken] = useState(null)
    const [loading, setLoading] = useState(true)

    // On first load check if user is already logged in
    useEffect(() => {
        const savedToken = localStorage.getItem('neciai_token')
        const savedUser = localStorage.getItem('neciai_user')
        if (savedToken && savedUser) {
            setToken(savedToken)
            setUser(JSON.parse(savedUser))
        }
        setLoading(false)
    }, [])

    /** Login — calls API, stores token and user in state and localStorage */
    const login = async (email, password) => {
        const response = await apiLogin({ email, password })
        const data = response.data
        setToken(data.token)
        setUser({
            userId: data.userId,
            email: data.email,
            fullName: data.fullName,
            role: data.role,
            expiresAt: data.expiresAt,
        })
        localStorage.setItem('neciai_token', data.token)
        localStorage.setItem('neciai_user', JSON.stringify({
            userId: data.userId,
            email: data.email,
            fullName: data.fullName,
            role: data.role,
        }))
        return data
    }

    /** Register — calls API then automatically logs in */
    const register = async (firstName, lastName, email, password, role) => {
        const response = await apiRegister({ firstName, lastName, email, password, role })
        const data = response.data
        setToken(data.token)
        setUser({
            userId: data.userId,
            email: data.email,
            fullName: data.fullName,
            role: data.role,
        })
        localStorage.setItem('neciai_token', data.token)
        localStorage.setItem('neciai_user', JSON.stringify({
            userId: data.userId,
            email: data.email,
            fullName: data.fullName,
            role: data.role,
        }))
        return data
    }

    /** Logout — clears all auth state */
    const logout = () => {
        setToken(null)
        setUser(null)
        localStorage.removeItem('neciai_token')
        localStorage.removeItem('neciai_user')
    }

    const value = {
        user, token, login, logout, register, loading,
        isAuthenticated: !!token
    }

    return (
        <AuthContext.Provider value={value}>
            {!loading && children}
        </AuthContext.Provider>
    )
}

/** Custom hook — makes consuming the context clean and simple */
export function useAuth() {
    const context = useContext(AuthContext)
    if (!context) throw new Error('useAuth must be used within AuthProvider')
    return context
}