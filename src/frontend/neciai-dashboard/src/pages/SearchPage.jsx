import { useState } from 'react'
import { searchRecords } from '../services/api'

export default function SearchPage() {
    const [keyword, setKeyword] = useState('')
    const [results, setResults] = useState([])
    const [searched, setSearched] = useState(false)
    const [loading, setLoading] = useState(false)
    const [error, setError] = useState('')

    const handleSearch = async (e) => {
        e.preventDefault()
        if (!keyword || keyword.trim().length < 2) { setError('Enter at least 2 characters.'); return }
        setError(''); setLoading(true)
        try {
            const res = await searchRecords(keyword.trim())
            setResults(res.data.results || []); setSearched(true)
        } catch { setError('Search failed.') }
        finally { setLoading(false) }
    }

    return (
        <div style={{ padding: '16px', maxWidth: '1200px', margin: '0 auto' }}>
            <h1 style={{ fontSize: '22px', fontWeight: 'bold', margin: '0 0 4px 0' }}>Search Records</h1>
            <p style={{ color: '#6b7280', fontSize: '13px', margin: '0 0 20px 0' }}>Search across titles, descriptions, categories, and tags</p>

            <form onSubmit={handleSearch} style={{ display: 'flex', gap: '10px', marginBottom: '20px', flexWrap: 'wrap' }}>
                <input value={keyword} onChange={e => setKeyword(e.target.value)} placeholder="Search financial records..."
                    style={{ flex: '1', minWidth: '200px', background: '#111827', border: '1px solid #374151', color: 'white', borderRadius: '8px', padding: '11px 16px', fontSize: '14px', outline: 'none' }} />
                <button type="submit" disabled={loading}
                    style={{ background: loading ? '#1e3a8a' : '#2563eb', color: 'white', border: 'none', borderRadius: '8px', padding: '11px 20px', fontSize: '14px', fontWeight: '600', cursor: 'pointer', whiteSpace: 'nowrap' }}>
                    {loading ? 'Searching...' : '🔍 Search'}
                </button>
            </form>

            {error && <div style={{ background: '#450a0a', border: '1px solid #7f1d1d', color: '#fca5a5', borderRadius: '8px', padding: '12px', marginBottom: '16px', fontSize: '13px' }}>{error}</div>}

            {searched && (
                <div style={{ background: '#111827', border: '1px solid #1f2937', borderRadius: '12px' }}>
                    <div style={{ padding: '14px 16px', borderBottom: '1px solid #1f2937' }}>
                        <h3 style={{ fontSize: '13px', fontWeight: '600', margin: 0 }}>{results.length} result{results.length !== 1 ? 's' : ''} for "{keyword}"</h3>
                    </div>
                    <div style={{ overflowX: 'auto', WebkitOverflowScrolling: 'touch' }}>
                        <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '13px', minWidth: '420px' }}>
                            <thead>
                                <tr style={{ color: '#6b7280', fontSize: '10px', textTransform: 'uppercase' }}>
                                    {['Date', 'Title', 'Category', 'Amount', 'Summary'].map(h => (
                                        <th key={h} style={{ padding: '10px 14px', textAlign: h === 'Amount' ? 'right' : 'left', borderBottom: '1px solid #1f2937', whiteSpace: 'nowrap' }}>{h}</th>
                                    ))}
                                </tr>
                            </thead>
                            <tbody>
                                {results.map(r => (
                                    <tr key={r.id} style={{ borderBottom: '1px solid #1f2937' }}>
                                        <td style={{ padding: '11px 14px', color: '#6b7280', whiteSpace: 'nowrap' }}>{new Date(r.recordDate).toLocaleDateString()}</td>
                                        <td style={{ padding: '11px 14px', color: 'white', fontWeight: '500', maxWidth: '150px', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{r.title}</td>
                                        <td style={{ padding: '11px 14px' }}>
                                            <span style={{ padding: '2px 8px', borderRadius: '9999px', fontSize: '11px', whiteSpace: 'nowrap', background: r.category === 'Revenue' ? '#064e3b' : r.category === 'Expense' ? '#450a0a' : '#1e3a8a', color: r.category === 'Revenue' ? '#34d399' : r.category === 'Expense' ? '#f87171' : '#60a5fa' }}>{r.category}</span>
                                        </td>
                                        <td style={{ padding: '11px 14px', textAlign: 'right', color: 'white', fontFamily: 'monospace', whiteSpace: 'nowrap' }}>${r.amount.toLocaleString()}</td>
                                        <td style={{ padding: '11px 14px', color: '#6b7280', fontSize: '11px', maxWidth: '200px', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{r.summary}</td>
                                    </tr>
                                ))}
                                {results.length === 0 && <tr><td colSpan={5} style={{ padding: '32px', textAlign: 'center', color: '#6b7280' }}>No records found for "{keyword}".</td></tr>}
                            </tbody>
                        </table>
                    </div>
                </div>
            )}

            {!searched && (
                <div style={{ textAlign: 'center', padding: '60px 0' }}>
                    <p style={{ fontSize: '40px', margin: '0 0 12px 0' }}>🔍</p>
                    <p style={{ color: '#6b7280', fontSize: '14px' }}>Enter a keyword above to search your financial records</p>
                </div>
            )}
        </div>
    )
}