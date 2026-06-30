/*
 * File:    LoginPage.jsx
 * Purpose: Login page for NeciAI. Fully responsive across all screen sizes.
 * Author:  Abraham Macias
 * Date:    June 2026
 */

import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

export default function LoginPage() {
    const [email, setEmail] = useState('')
    const [password, setPassword] = useState('')
    const [error, setError] = useState('')
    const [loading, setLoading] = useState(false)
    const { login } = useAuth()
    const navigate = useNavigate()

    const handleSubmit = async (e) => {
        e.preventDefault()
        setError('')
        if (!email || !password) { setError('Please enter your email and password.'); return }
        setLoading(true)
        try {
            await login(email, password)
            navigate('/dashboard')
        } catch (err) {
            setError(err.response?.data?.message || 'Login failed. Please try again.')
        } finally {
            setLoading(false)
        }
    }

    return (
        <div style={{
            minHeight: '100dvh',
            background: '#030712',
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            justifyContent: 'center',
            padding: '24px 16px',
        }}>
            <div style={{ width: '100%', maxWidth: '420px' }}>

                {/* Logo and Title — OUTSIDE the card, centered above */}
                <div style={{ textAlign: 'center', marginBottom: '32px' }}>
                    <h1 style={{
                        fontSize: '42px',
                        fontWeight: 'bold',
                        color: 'white',
                        margin: '0 0 8px 0',
                        letterSpacing: '-0.5px',
                    }}>
                        NeciAI
                    </h1>
                    <p style={{
                        color: '#6b7280',
                        fontSize: '14px',
                        margin: 0,
                    }}>
                        Intelligent Financial Data Analysis Platform
                    </p>
                </div>

                {/* Card */}
                <div style={{
                    background: '#111827',
                    borderRadius: '16px',
                    padding: '32px',
                    border: '1px solid #1f2937',
                    boxShadow: '0 25px 50px -12px rgba(0,0,0,0.8)',
                }}>
                    <h2 style={{
                        color: 'white',
                        fontSize: '20px',
                        fontWeight: '600',
                        margin: '0 0 24px 0',
                    }}>
                        Sign in to your account
                    </h2>

                    {/* Error */}
                    {error && (
                        <div style={{
                            background: 'rgba(127,29,29,0.4)',
                            border: '1px solid #7f1d1d',
                            color: '#fca5a5',
                            borderRadius: '8px',
                            padding: '12px 16px',
                            marginBottom: '16px',
                            fontSize: '14px',
                        }}>
                            {error}
                        </div>
                    )}

                    <form onSubmit={handleSubmit}>

                        {/* Email */}
                        <div style={{ marginBottom: '16px' }}>
                            <label style={{
                                display: 'block',
                                color: '#d1d5db',
                                fontSize: '14px',
                                fontWeight: '500',
                                marginBottom: '6px',
                            }}>
                                Email address
                            </label>
                            <input
                                type="email"
                                value={email}
                                onChange={e => setEmail(e.target.value)}
                                placeholder="you@example.com"
                                required
                                style={{
                                    width: '100%',
                                    background: '#1f2937',
                                    border: '1px solid #374151',
                                    color: 'white',
                                    borderRadius: '8px',
                                    padding: '11px 14px',
                                    fontSize: '14px',
                                    boxSizing: 'border-box',
                                    outline: 'none',
                                    transition: 'border-color 0.15s',
                                }}
                                onFocus={e => e.target.style.borderColor = '#3b82f6'}
                                onBlur={e => e.target.style.borderColor = '#374151'}
                            />
                        </div>

                        {/* Password */}
                        <div style={{ marginBottom: '20px' }}>
                            <label style={{
                                display: 'block',
                                color: '#d1d5db',
                                fontSize: '14px',
                                fontWeight: '500',
                                marginBottom: '6px',
                            }}>
                                Password
                            </label>
                            <input
                                type="password"
                                value={password}
                                onChange={e => setPassword(e.target.value)}
                                placeholder="••••••••"
                                required
                                style={{
                                    width: '100%',
                                    background: '#1f2937',
                                    border: '1px solid #374151',
                                    color: 'white',
                                    borderRadius: '8px',
                                    padding: '11px 14px',
                                    fontSize: '14px',
                                    boxSizing: 'border-box',
                                    outline: 'none',
                                    transition: 'border-color 0.15s',
                                }}
                                onFocus={e => e.target.style.borderColor = '#3b82f6'}
                                onBlur={e => e.target.style.borderColor = '#374151'}
                            />
                        </div>

                        {/* Submit Button */}
                        <button
                            type="submit"
                            disabled={loading}
                            style={{
                                width: '100%',
                                background: loading ? '#1d4ed8' : '#2563eb',
                                color: 'white',
                                border: 'none',
                                borderRadius: '8px',
                                padding: '12px',
                                fontSize: '15px',
                                fontWeight: '600',
                                cursor: loading ? 'not-allowed' : 'pointer',
                                transition: 'background 0.15s',
                                letterSpacing: '0.01em',
                            }}
                            onMouseEnter={e => { if (!loading) e.target.style.background = '#1d4ed8' }}
                            onMouseLeave={e => { if (!loading) e.target.style.background = '#2563eb' }}
                        >
                            {loading ? 'Signing in...' : 'Sign in'}
                        </button>
                    </form>

                    {/* Demo Credentials Box */}
                    <div style={{
                        marginTop: '20px',
                        background: '#1f2937',
                        borderRadius: '8px',
                        padding: '14px 16px',
                        border: '1px solid #374151',
                    }}>
                        <p style={{ color: '#9ca3af', fontSize: '12px', fontWeight: '600', margin: '0 0 6px 0', textTransform: 'uppercase', letterSpacing: '0.05em' }}>
                            Demo credentials
                        </p>
                        <p style={{ color: '#d1d5db', fontSize: '13px', margin: '0 0 3px 0' }}>
                            Email: <span style={{ color: 'white', fontWeight: '500' }}>admin@neciai.app</span>
                        </p>
                        <p style={{ color: '#d1d5db', fontSize: '13px', margin: 0 }}>
                            Password: <span style={{ color: 'white', fontWeight: '500' }}>Admin@NeciAI2026!</span>
                        </p>
                    </div>
                </div>

                {/* Etymology note */}
                <p style={{
                    textAlign: 'center',
                    color: '#374151',
                    fontSize: '12px',
                    marginTop: '24px',
                    fontStyle: 'italic',
                }}>
                    NeciAI — from <em>Neciz</em>, Nahuatl for "to become clear"
                </p>
            </div>
        </div>
    )
}