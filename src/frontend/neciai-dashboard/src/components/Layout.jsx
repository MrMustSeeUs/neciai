import { useState } from 'react'
import { Outlet, NavLink, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

const navItems = [
    { to: '/dashboard', label: 'Dashboard', icon: '📊' },
    { to: '/records', label: 'Records', icon: '📋' },
    { to: '/reports', label: 'Reports', icon: '📄' },
    { to: '/search', label: 'Search', icon: '🔍' },
]

export default function Layout() {
    const { user, logout } = useAuth()
    const navigate = useNavigate()
    const [menuOpen, setMenuOpen] = useState(false)

    const handleLogout = () => {
        logout()
        navigate('/login')
    }

    const closeMenu = () => setMenuOpen(false)

    return (
        <div style={{ display: 'flex', height: '100dvh', background: '#030712', color: 'white', overflow: 'hidden' }}>

            {/* Mobile overlay */}
            {menuOpen && (
                <div
                    onClick={closeMenu}
                    style={{ position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.6)', zIndex: 40 }}
                />
            )}

            {/* Sidebar */}
            <aside style={{
                width: '240px',
                minWidth: '240px',
                background: '#111827',
                borderRight: '1px solid #1f2937',
                display: 'flex',
                flexDirection: 'column',
                position: 'fixed',
                top: 0,
                left: 0,
                height: '100dvh',
                zIndex: 50,
                transform: menuOpen ? 'translateX(0)' : 'translateX(-100%)',
                transition: 'transform 0.25s ease',
            }}
                className="sidebar">
                <div style={{ padding: '20px 24px', borderBottom: '1px solid #1f2937', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <div>
                        <h1 style={{ fontSize: '22px', fontWeight: 'bold', margin: 0 }}>NeciAI</h1>
                        <p style={{ fontSize: '11px', color: '#6b7280', margin: '4px 0 0 0' }}>Financial Intelligence</p>
                    </div>
                    <button onClick={closeMenu} style={{ background: 'none', border: 'none', color: '#6b7280', fontSize: '20px', cursor: 'pointer', padding: '4px' }}>✕</button>
                </div>

                <nav style={{ flex: 1, padding: '12px' }}>
                    {navItems.map((item) => (
                        <NavLink key={item.to} to={item.to} onClick={closeMenu}
                            style={({ isActive }) => ({
                                display: 'flex', alignItems: 'center', gap: '10px',
                                padding: '12px 14px', borderRadius: '8px', marginBottom: '4px',
                                textDecoration: 'none', fontSize: '15px', fontWeight: '500',
                                background: isActive ? '#2563eb' : 'transparent',
                                color: isActive ? 'white' : '#9ca3af',
                            })}>
                            <span style={{ fontSize: '18px' }}>{item.icon}</span>
                            {item.label}
                        </NavLink>
                    ))}
                </nav>

                <div style={{ padding: '16px', borderTop: '1px solid #1f2937' }}>
                    <p style={{ fontSize: '13px', fontWeight: '500', margin: '0 0 2px 0', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{user?.fullName}</p>
                    <p style={{ fontSize: '11px', color: '#6b7280', margin: '0 0 4px 0', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{user?.email}</p>
                    <span style={{ display: 'inline-block', background: '#1e3a8a', color: '#93c5fd', fontSize: '10px', padding: '2px 8px', borderRadius: '9999px', marginBottom: '10px' }}>{user?.role}</span>
                    <button onClick={handleLogout} style={{
                        width: '100%', background: 'transparent', border: '1px solid #374151',
                        color: '#9ca3af', padding: '8px', borderRadius: '6px',
                        fontSize: '13px', cursor: 'pointer',
                    }}>Sign out</button>
                </div>
            </aside>

            {/* Desktop sidebar — always visible on large screens */}
            <style>{`
        @media (min-width: 768px) {
          .sidebar {
            transform: translateX(0) !important;
            position: relative !important;
          }
          .mobile-header { display: none !important; }
          .main-content { margin-left: 0 !important; }
        }
        @media (max-width: 767px) {
          .sidebar { position: fixed !important; }
        }
      `}</style>

            {/* Main area */}
            <div style={{ flex: 1, display: 'flex', flexDirection: 'column', overflow: 'hidden', minWidth: 0 }}>

                {/* Mobile header */}
                <div className="mobile-header" style={{
                    display: 'flex', alignItems: 'center', justifyContent: 'space-between',
                    padding: '12px 16px', background: '#111827', borderBottom: '1px solid #1f2937',
                    position: 'sticky', top: 0, zIndex: 30,
                }}>
                    <button
                        onClick={() => setMenuOpen(true)}
                        style={{ background: 'none', border: 'none', color: 'white', fontSize: '22px', cursor: 'pointer', padding: '4px' }}>
                        ☰
                    </button>
                    <span style={{ fontSize: '18px', fontWeight: 'bold' }}>NeciAI</span>
                    <span style={{ fontSize: '11px', color: '#6b7280' }}>{user?.role}</span>
                </div>

                {/* Page content */}
                <main style={{ flex: 1, overflow: 'auto', WebkitOverflowScrolling: 'touch' }}>
                    <Outlet />
                </main>
            </div>
        </div>
    )
}