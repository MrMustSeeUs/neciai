import { useState, useEffect } from 'react'
import { getRecords } from '../services/api'
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, BarChart, Bar, Legend } from 'recharts'

export default function DashboardPage() {
    const [records, setRecords] = useState([])
    const [loading, setLoading] = useState(true)

    useEffect(() => {
        getRecords().then(res => setRecords(res.data)).catch(() => { }).finally(() => setLoading(false))
    }, [])

    const totalRevenue = records.filter(r => r.category === 'Revenue').reduce((s, r) => s + r.amount, 0)
    const totalExpenses = records.filter(r => r.category === 'Expense').reduce((s, r) => s + r.amount, 0)
    const netPosition = totalRevenue - totalExpenses
    const anomalies = records.filter(r => r.isAnomaly).length

    const chartData = records.reduce((acc, record) => {
        const month = new Date(record.recordDate).toLocaleString('default', { month: 'short', year: '2-digit' })
        const existing = acc.find(d => d.month === month)
        if (existing) {
            if (record.category === 'Revenue') existing.revenue += record.amount
            if (record.category === 'Expense') existing.expenses += record.amount
        } else {
            acc.push({ month, revenue: record.category === 'Revenue' ? record.amount : 0, expenses: record.category === 'Expense' ? record.amount : 0 })
        }
        return acc
    }, []).slice(-6)

    const cards = [
        { label: 'Total Revenue', value: `$${totalRevenue.toLocaleString()}`, color: '#34d399', bg: '#064e3b' },
        { label: 'Total Expenses', value: `$${totalExpenses.toLocaleString()}`, color: '#f87171', bg: '#450a0a' },
        { label: 'Net Position', value: `$${netPosition.toLocaleString()}`, color: netPosition >= 0 ? '#60a5fa' : '#fb923c', bg: '#1e3a8a' },
        { label: 'Anomalies', value: anomalies, color: '#facc15', bg: '#422006' },
    ]

    if (loading) return (
        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', height: '100%', color: '#6b7280' }}>
            Loading dashboard...
        </div>
    )

    return (
        <div style={{ padding: '16px', maxWidth: '1200px', margin: '0 auto' }}>
            <h1 style={{ fontSize: '22px', fontWeight: 'bold', margin: '0 0 4px 0' }}>Dashboard</h1>
            <p style={{ color: '#6b7280', fontSize: '13px', margin: '0 0 20px 0' }}>Financial overview and performance metrics</p>

            {/* Metric Cards */}
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(140px, 1fr))', gap: '12px', marginBottom: '20px' }}>
                {cards.map(card => (
                    <div key={card.label} style={{ background: card.bg, border: `1px solid ${card.color}33`, borderRadius: '12px', padding: '16px' }}>
                        <p style={{ color: '#9ca3af', fontSize: '11px', textTransform: 'uppercase', letterSpacing: '0.05em', margin: '0 0 6px 0' }}>{card.label}</p>
                        <p style={{ color: card.color, fontSize: '20px', fontWeight: 'bold', margin: 0, wordBreak: 'break-all' }}>{card.value}</p>
                    </div>
                ))}
            </div>

            {/* Charts */}
            {chartData.length > 0 && (
                <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(280px, 1fr))', gap: '16px', marginBottom: '20px' }}>
                    <div style={{ background: '#111827', border: '1px solid #1f2937', borderRadius: '12px', padding: '16px', minWidth: 0 }}>
                        <h3 style={{ fontSize: '13px', fontWeight: '600', margin: '0 0 12px 0' }}>Revenue vs Expenses</h3>
                        <ResponsiveContainer width="100%" height={180}>
                            <LineChart data={chartData}>
                                <CartesianGrid strokeDasharray="3 3" stroke="#1f2937" />
                                <XAxis dataKey="month" stroke="#6b7280" tick={{ fontSize: 10 }} />
                                <YAxis stroke="#6b7280" tick={{ fontSize: 10 }} width={50} />
                                <Tooltip contentStyle={{ background: '#1f2937', border: '1px solid #374151', borderRadius: '8px', fontSize: '12px' }} />
                                <Legend wrapperStyle={{ fontSize: '11px' }} />
                                <Line type="monotone" dataKey="revenue" stroke="#34d399" strokeWidth={2} dot={false} />
                                <Line type="monotone" dataKey="expenses" stroke="#f87171" strokeWidth={2} dot={false} />
                            </LineChart>
                        </ResponsiveContainer>
                    </div>
                    <div style={{ background: '#111827', border: '1px solid #1f2937', borderRadius: '12px', padding: '16px', minWidth: 0 }}>
                        <h3 style={{ fontSize: '13px', fontWeight: '600', margin: '0 0 12px 0' }}>Monthly Breakdown</h3>
                        <ResponsiveContainer width="100%" height={180}>
                            <BarChart data={chartData}>
                                <CartesianGrid strokeDasharray="3 3" stroke="#1f2937" />
                                <XAxis dataKey="month" stroke="#6b7280" tick={{ fontSize: 10 }} />
                                <YAxis stroke="#6b7280" tick={{ fontSize: 10 }} width={50} />
                                <Tooltip contentStyle={{ background: '#1f2937', border: '1px solid #374151', borderRadius: '8px', fontSize: '12px' }} />
                                <Legend wrapperStyle={{ fontSize: '11px' }} />
                                <Bar dataKey="revenue" fill="#34d399" radius={[4, 4, 0, 0]} />
                                <Bar dataKey="expenses" fill="#f87171" radius={[4, 4, 0, 0]} />
                            </BarChart>
                        </ResponsiveContainer>
                    </div>
                </div>
            )}

            {/* Recent Records */}
            <div style={{ background: '#111827', border: '1px solid #1f2937', borderRadius: '12px' }}>
                <div style={{ padding: '14px 16px', borderBottom: '1px solid #1f2937' }}>
                    <h3 style={{ fontSize: '13px', fontWeight: '600', margin: 0 }}>Recent Records</h3>
                </div>
                <div style={{ overflowX: 'auto', WebkitOverflowScrolling: 'touch' }}>
                    <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '13px', minWidth: '480px' }}>
                        <thead>
                            <tr style={{ color: '#6b7280', fontSize: '10px', textTransform: 'uppercase' }}>
                                {['Date', 'Title', 'Category', 'Amount', 'Anomaly'].map(h => (
                                    <th key={h} style={{ padding: '10px 14px', textAlign: h === 'Amount' ? 'right' : h === 'Anomaly' ? 'center' : 'left', borderBottom: '1px solid #1f2937', whiteSpace: 'nowrap' }}>{h}</th>
                                ))}
                            </tr>
                        </thead>
                        <tbody>
                            {records.slice(0, 10).map(r => (
                                <tr key={r.id} style={{ borderBottom: '1px solid #111827' }}>
                                    <td style={{ padding: '11px 14px', color: '#6b7280', whiteSpace: 'nowrap' }}>{new Date(r.recordDate).toLocaleDateString()}</td>
                                    <td style={{ padding: '11px 14px', color: 'white', fontWeight: '500', maxWidth: '180px', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{r.title}</td>
                                    <td style={{ padding: '11px 14px' }}>
                                        <span style={{ padding: '2px 8px', borderRadius: '9999px', fontSize: '11px', fontWeight: '500', background: r.category === 'Revenue' ? '#064e3b' : r.category === 'Expense' ? '#450a0a' : '#1e3a8a', color: r.category === 'Revenue' ? '#34d399' : r.category === 'Expense' ? '#f87171' : '#60a5fa', whiteSpace: 'nowrap' }}>{r.category}</span>
                                    </td>
                                    <td style={{ padding: '11px 14px', textAlign: 'right', color: 'white', fontFamily: 'monospace', whiteSpace: 'nowrap' }}>${r.amount.toLocaleString()}</td>
                                    <td style={{ padding: '11px 14px', textAlign: 'center' }}>{r.isAnomaly ? '⚠' : '—'}</td>
                                </tr>
                            ))}
                            {records.length === 0 && (
                                <tr><td colSpan={5} style={{ padding: '32px', textAlign: 'center', color: '#6b7280' }}>No records yet.</td></tr>
                            )}
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    )
}