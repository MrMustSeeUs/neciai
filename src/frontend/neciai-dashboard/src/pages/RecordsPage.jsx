import { useState, useEffect } from 'react'
import { getRecords, createRecord, updateRecord, deleteRecord } from '../services/api'

const CATEGORIES = ['Revenue', 'Expense', 'Budget']
const emptyForm = { title: '', description: '', category: 'Revenue', amount: '', recordDate: '', tags: '' }

export default function RecordsPage() {
    const [records, setRecords] = useState([])
    const [loading, setLoading] = useState(true)
    const [showForm, setShowForm] = useState(false)
    const [form, setForm] = useState(emptyForm)
    const [editing, setEditing] = useState(null)
    const [error, setError] = useState('')
    const [success, setSuccess] = useState('')

    useEffect(() => { load() }, [])

    const load = () => {
        setLoading(true)
        getRecords().then(res => setRecords(res.data)).catch(() => setError('Failed to load.')).finally(() => setLoading(false))
    }

    const handleSubmit = async (e) => {
        e.preventDefault()
        setError(''); setSuccess('')
        if (!form.title || !form.amount || !form.recordDate) { setError('Title, amount, and date are required.'); return }
        if (parseFloat(form.amount) <= 0) { setError('Amount must be greater than zero.'); return }
        try {
            const payload = { ...form, amount: parseFloat(form.amount), recordDate: new Date(form.recordDate).toISOString() }
            if (editing) { await updateRecord(editing, payload); setSuccess('Record updated.') }
            else { await createRecord(payload); setSuccess('Record created.') }
            setForm(emptyForm); setEditing(null); setShowForm(false); load()
        } catch (err) { setError(err.response?.data?.message || 'Operation failed.') }
    }

    const handleEdit = (r) => {
        setForm({ title: r.title, description: r.description, category: r.category, amount: r.amount.toString(), recordDate: r.recordDate.split('T')[0], tags: r.tags })
        setEditing(r.id); setShowForm(true)
        setTimeout(() => window.scrollTo({ top: 0, behavior: 'smooth' }), 100)
    }

    const handleDelete = async (id) => {
        if (!window.confirm('Delete this record?')) return
        try { await deleteRecord(id); setSuccess('Deleted.'); load() } catch { setError('Failed to delete.') }
    }

    const inp = { style: { width: '100%', background: '#1f2937', border: '1px solid #374151', color: 'white', borderRadius: '8px', padding: '10px 12px', fontSize: '14px', boxSizing: 'border-box' } }

    return (
        <div style={{ padding: '16px', maxWidth: '1200px', margin: '0 auto' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '20px', gap: '12px', flexWrap: 'wrap' }}>
                <div>
                    <h1 style={{ fontSize: '22px', fontWeight: 'bold', margin: '0 0 4px 0' }}>Financial Records</h1>
                    <p style={{ color: '#6b7280', fontSize: '13px', margin: 0 }}>Manage budgets, expenses, and revenue</p>
                </div>
                <button onClick={() => { setShowForm(!showForm); setEditing(null); setForm(emptyForm) }}
                    style={{ background: '#2563eb', color: 'white', border: 'none', borderRadius: '8px', padding: '10px 16px', fontSize: '14px', fontWeight: '600', cursor: 'pointer', whiteSpace: 'nowrap' }}>
                    {showForm ? 'Cancel' : '+ Add Record'}
                </button>
            </div>

            {error && <div style={{ background: '#450a0a', border: '1px solid #7f1d1d', color: '#fca5a5', borderRadius: '8px', padding: '12px', marginBottom: '16px', fontSize: '13px' }}>{error}</div>}
            {success && <div style={{ background: '#052e16', border: '1px solid #14532d', color: '#86efac', borderRadius: '8px', padding: '12px', marginBottom: '16px', fontSize: '13px' }}>{success}</div>}

            {showForm && (
                <div style={{ background: '#111827', border: '1px solid #1f2937', borderRadius: '12px', padding: '20px', marginBottom: '20px' }}>
                    <h3 style={{ fontSize: '14px', fontWeight: '600', margin: '0 0 16px 0' }}>{editing ? 'Edit Record' : 'New Record'}</h3>
                    <form onSubmit={handleSubmit}>
                        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '12px' }}>
                            <div style={{ gridColumn: '1 / -1' }}>
                                <label style={{ display: 'block', color: '#9ca3af', fontSize: '12px', marginBottom: '4px' }}>Title *</label>
                                <input value={form.title} onChange={e => setForm({ ...form, title: e.target.value })} placeholder="Record title" {...inp} />
                            </div>
                            <div>
                                <label style={{ display: 'block', color: '#9ca3af', fontSize: '12px', marginBottom: '4px' }}>Category *</label>
                                <select value={form.category} onChange={e => setForm({ ...form, category: e.target.value })} {...inp}>
                                    {CATEGORIES.map(c => <option key={c}>{c}</option>)}
                                </select>
                            </div>
                            <div>
                                <label style={{ display: 'block', color: '#9ca3af', fontSize: '12px', marginBottom: '4px' }}>Amount *</label>
                                <input type="number" min="0.01" step="0.01" value={form.amount} onChange={e => setForm({ ...form, amount: e.target.value })} placeholder="0.00" {...inp} />
                            </div>
                            <div>
                                <label style={{ display: 'block', color: '#9ca3af', fontSize: '12px', marginBottom: '4px' }}>Date *</label>
                                <input type="date" value={form.recordDate} onChange={e => setForm({ ...form, recordDate: e.target.value })} {...inp} />
                            </div>
                            <div>
                                <label style={{ display: 'block', color: '#9ca3af', fontSize: '12px', marginBottom: '4px' }}>Tags</label>
                                <input value={form.tags} onChange={e => setForm({ ...form, tags: e.target.value })} placeholder="Q1,2026" {...inp} />
                            </div>
                            <div style={{ gridColumn: '1 / -1' }}>
                                <label style={{ display: 'block', color: '#9ca3af', fontSize: '12px', marginBottom: '4px' }}>Description</label>
                                <textarea value={form.description} onChange={e => setForm({ ...form, description: e.target.value })} rows={2}
                                    style={{ width: '100%', background: '#1f2937', border: '1px solid #374151', color: 'white', borderRadius: '8px', padding: '10px 12px', fontSize: '14px', boxSizing: 'border-box', resize: 'vertical' }} />
                            </div>
                        </div>
                        <button type="submit" style={{ marginTop: '14px', background: '#2563eb', color: 'white', border: 'none', borderRadius: '8px', padding: '10px 24px', fontSize: '14px', fontWeight: '600', cursor: 'pointer' }}>
                            {editing ? 'Update Record' : 'Create Record'}
                        </button>
                    </form>
                </div>
            )}

            <div style={{ background: '#111827', border: '1px solid #1f2937', borderRadius: '12px' }}>
                <div style={{ padding: '14px 16px', borderBottom: '1px solid #1f2937' }}>
                    <h3 style={{ fontSize: '13px', fontWeight: '600', margin: 0 }}>All Records ({records.length})</h3>
                </div>
                {loading ? <p style={{ textAlign: 'center', color: '#6b7280', padding: '32px' }}>Loading...</p> : (
                    <div style={{ overflowX: 'auto', WebkitOverflowScrolling: 'touch' }}>
                        <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '13px', minWidth: '520px' }}>
                            <thead>
                                <tr style={{ color: '#6b7280', fontSize: '10px', textTransform: 'uppercase' }}>
                                    {['Date', 'Title', 'Category', 'Amount', 'Anomaly', 'Actions'].map(h => (
                                        <th key={h} style={{ padding: '10px 14px', textAlign: h === 'Amount' ? 'right' : h === 'Anomaly' || h === 'Actions' ? 'center' : 'left', borderBottom: '1px solid #1f2937', whiteSpace: 'nowrap' }}>{h}</th>
                                    ))}
                                </tr>
                            </thead>
                            <tbody>
                                {records.map(r => (
                                    <tr key={r.id} style={{ borderBottom: '1px solid #1f2937' }}>
                                        <td style={{ padding: '11px 14px', color: '#6b7280', whiteSpace: 'nowrap' }}>{new Date(r.recordDate).toLocaleDateString()}</td>
                                        <td style={{ padding: '11px 14px', color: 'white', fontWeight: '500', maxWidth: '160px', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{r.title}</td>
                                        <td style={{ padding: '11px 14px' }}>
                                            <span style={{ padding: '2px 8px', borderRadius: '9999px', fontSize: '11px', whiteSpace: 'nowrap', background: r.category === 'Revenue' ? '#064e3b' : r.category === 'Expense' ? '#450a0a' : '#1e3a8a', color: r.category === 'Revenue' ? '#34d399' : r.category === 'Expense' ? '#f87171' : '#60a5fa' }}>{r.category}</span>
                                        </td>
                                        <td style={{ padding: '11px 14px', textAlign: 'right', color: 'white', fontFamily: 'monospace', whiteSpace: 'nowrap' }}>${r.amount.toLocaleString()}</td>
                                        <td style={{ padding: '11px 14px', textAlign: 'center' }}>{r.isAnomaly ? '⚠' : '—'}</td>
                                        <td style={{ padding: '11px 14px', textAlign: 'center', whiteSpace: 'nowrap' }}>
                                            <button onClick={() => handleEdit(r)} style={{ background: 'none', border: 'none', color: '#60a5fa', cursor: 'pointer', fontSize: '13px', marginRight: '10px' }}>Edit</button>
                                            <button onClick={() => handleDelete(r.id)} style={{ background: 'none', border: 'none', color: '#f87171', cursor: 'pointer', fontSize: '13px' }}>Delete</button>
                                        </td>
                                    </tr>
                                ))}
                                {records.length === 0 && <tr><td colSpan={6} style={{ padding: '32px', textAlign: 'center', color: '#6b7280' }}>No records yet.</td></tr>}
                            </tbody>
                        </table>
                    </div>
                )}
            </div>
        </div>
    )
}