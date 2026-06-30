import { useState, useEffect } from 'react'
import { getReports, generatePdfReport, generateExcelReport, downloadReport, deleteReport } from '../services/api'

export default function ReportsPage() {
    const [reports, setReports] = useState([])
    const [loading, setLoading] = useState(true)
    const [generating, setGenerating] = useState(false)
    const [error, setError] = useState('')
    const [success, setSuccess] = useState('')
    const [form, setForm] = useState({ title: '', startDate: '', endDate: '', format: 'PDF' })

    useEffect(() => { load() }, [])

    const load = () => {
        getReports().then(res => setReports(res.data)).catch(() => setError('Failed to load.')).finally(() => setLoading(false))
    }

    const handleGenerate = async (e) => {
        e.preventDefault()
        if (!form.title || !form.startDate || !form.endDate) { setError('All fields required.'); return }
        setError(''); setGenerating(true)
        try {
            if (form.format === 'PDF') await generatePdfReport(form.title, form.startDate, form.endDate)
            else await generateExcelReport(form.title, form.startDate, form.endDate)
            setSuccess(`${form.format} report generated!`)
            setForm({ title: '', startDate: '', endDate: '', format: 'PDF' }); load()
        } catch (err) { setError(err.response?.data?.message || 'Failed.') }
        finally { setGenerating(false) }
    }

    const handleDownload = async (id, title, format) => {
        try {
            const res = await downloadReport(id)
            const url = window.URL.createObjectURL(new Blob([res.data]))
            const a = document.createElement('a'); a.href = url
            a.setAttribute('download', `${title.replace(/\s+/g, '_')}.${format === 'PDF' ? 'pdf' : 'xlsx'}`)
            document.body.appendChild(a); a.click(); a.remove()
        } catch { setError('Download failed.') }
    }

    const handleDelete = async (id) => {
        if (!window.confirm('Delete this report?')) return
        try { await deleteReport(id); setSuccess('Deleted.'); load() } catch { setError('Failed.') }
    }

    const inp = { style: { width: '100%', background: '#1f2937', border: '1px solid #374151', color: 'white', borderRadius: '8px', padding: '10px 12px', fontSize: '14px', boxSizing: 'border-box' } }

    return (
        <div style={{ padding: '16px', maxWidth: '1200px', margin: '0 auto' }}>
            <h1 style={{ fontSize: '22px', fontWeight: 'bold', margin: '0 0 4px 0' }}>Reports</h1>
            <p style={{ color: '#6b7280', fontSize: '13px', margin: '0 0 20px 0' }}>Generate and download financial reports</p>

            {error && <div style={{ background: '#450a0a', border: '1px solid #7f1d1d', color: '#fca5a5', borderRadius: '8px', padding: '12px', marginBottom: '16px', fontSize: '13px' }}>{error}</div>}
            {success && <div style={{ background: '#052e16', border: '1px solid #14532d', color: '#86efac', borderRadius: '8px', padding: '12px', marginBottom: '16px', fontSize: '13px' }}>{success}</div>}

            <div style={{ background: '#111827', border: '1px solid #1f2937', borderRadius: '12px', padding: '20px', marginBottom: '20px' }}>
                <h3 style={{ fontSize: '14px', fontWeight: '600', margin: '0 0 16px 0' }}>Generate New Report</h3>
                <form onSubmit={handleGenerate}>
                    <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '12px' }}>
                        <div style={{ gridColumn: '1 / -1' }}>
                            <label style={{ display: 'block', color: '#9ca3af', fontSize: '12px', marginBottom: '4px' }}>Report Title *</label>
                            <input value={form.title} onChange={e => setForm({ ...form, title: e.target.value })} placeholder="Q2 2026 Financial Summary" {...inp} />
                        </div>
                        <div>
                            <label style={{ display: 'block', color: '#9ca3af', fontSize: '12px', marginBottom: '4px' }}>Start Date *</label>
                            <input type="date" value={form.startDate} onChange={e => setForm({ ...form, startDate: e.target.value })} {...inp} />
                        </div>
                        <div>
                            <label style={{ display: 'block', color: '#9ca3af', fontSize: '12px', marginBottom: '4px' }}>End Date *</label>
                            <input type="date" value={form.endDate} onChange={e => setForm({ ...form, endDate: e.target.value })} {...inp} />
                        </div>
                        <div>
                            <label style={{ display: 'block', color: '#9ca3af', fontSize: '12px', marginBottom: '4px' }}>Format</label>
                            <select value={form.format} onChange={e => setForm({ ...form, format: e.target.value })} {...inp}>
                                <option value="PDF">PDF</option>
                                <option value="Excel">Excel</option>
                            </select>
                        </div>
                        <div style={{ display: 'flex', alignItems: 'flex-end' }}>
                            <button type="submit" disabled={generating}
                                style={{ width: '100%', background: generating ? '#1e3a8a' : '#2563eb', color: 'white', border: 'none', borderRadius: '8px', padding: '10px 20px', fontSize: '14px', fontWeight: '600', cursor: 'pointer' }}>
                                {generating ? 'Generating...' : 'Generate Report'}
                            </button>
                        </div>
                    </div>
                </form>
            </div>

            <div style={{ background: '#111827', border: '1px solid #1f2937', borderRadius: '12px' }}>
                <div style={{ padding: '14px 16px', borderBottom: '1px solid #1f2937' }}>
                    <h3 style={{ fontSize: '13px', fontWeight: '600', margin: 0 }}>Generated Reports ({reports.length})</h3>
                </div>
                {loading ? <p style={{ textAlign: 'center', color: '#6b7280', padding: '32px' }}>Loading...</p> : (
                    <div style={{ overflowX: 'auto', WebkitOverflowScrolling: 'touch' }}>
                        <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '13px', minWidth: '480px' }}>
                            <thead>
                                <tr style={{ color: '#6b7280', fontSize: '10px', textTransform: 'uppercase' }}>
                                    {['Title', 'Format', 'Generated', 'Rows', 'Actions'].map(h => (
                                        <th key={h} style={{ padding: '10px 14px', textAlign: h === 'Rows' || h === 'Actions' ? 'center' : 'left', borderBottom: '1px solid #1f2937', whiteSpace: 'nowrap' }}>{h}</th>
                                    ))}
                                </tr>
                            </thead>
                            <tbody>
                                {reports.map(r => (
                                    <tr key={r.id} style={{ borderBottom: '1px solid #1f2937' }}>
                                        <td style={{ padding: '11px 14px', color: 'white', fontWeight: '500', maxWidth: '180px', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{r.title}</td>
                                        <td style={{ padding: '11px 14px' }}>
                                            <span style={{ padding: '2px 8px', borderRadius: '9999px', fontSize: '11px', background: r.format === 'PDF' ? '#450a0a' : '#052e16', color: r.format === 'PDF' ? '#f87171' : '#34d399' }}>{r.format}</span>
                                        </td>
                                        <td style={{ padding: '11px 14px', color: '#9ca3af', whiteSpace: 'nowrap' }}>{new Date(r.generatedAt).toLocaleString()}</td>
                                        <td style={{ padding: '11px 14px', textAlign: 'center', color: '#9ca3af' }}>{r.rowCount}</td>
                                        <td style={{ padding: '11px 14px', textAlign: 'center', whiteSpace: 'nowrap' }}>
                                            <button onClick={() => handleDownload(r.id, r.title, r.format)} style={{ background: 'none', border: 'none', color: '#60a5fa', cursor: 'pointer', fontSize: '13px', marginRight: '10px' }}>Download</button>
                                            <button onClick={() => handleDelete(r.id)} style={{ background: 'none', border: 'none', color: '#f87171', cursor: 'pointer', fontSize: '13px' }}>Delete</button>
                                        </td>
                                    </tr>
                                ))}
                                {reports.length === 0 && <tr><td colSpan={5} style={{ padding: '32px', textAlign: 'center', color: '#6b7280' }}>No reports yet.</td></tr>}
                            </tbody>
                        </table>
                    </div>
                )}
            </div>
        </div>
    )
}