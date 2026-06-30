/**
 * api.js
 *
 * Centralized Axios HTTP client for the NeciAI frontend.
 * All API requests flow through this module so that authentication
 * headers, base URL configuration, and error handling are applied
 * consistently across the application.
 *
 * Author: Abraham Macias
 */

import axios from 'axios'

// Resolve the API base URL from the Vite environment variable injected
// at build time, falling back to the Vite dev proxy for local development.
const BASE_URL = import.meta.env.VITE_API_URL
  ? `${import.meta.env.VITE_API_URL}/api`
  : '/api'

const api = axios.create({
  baseURL: BASE_URL,
  headers: { 'Content-Type': 'application/json' },
  timeout: 15000,
})

// ── REQUEST INTERCEPTOR ───────────────────────────────────────
// Attach the JWT Bearer token from localStorage to every outgoing
// request. The token is stored on login and cleared on logout.
api.interceptors.request.use(
  config => {
    const token = localStorage.getItem('token')
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  error => Promise.reject(error)
)

// ── RESPONSE INTERCEPTOR ──────────────────────────────────────
// Handle 401 Unauthorized responses globally by clearing the stored
// token and redirecting to the login page. This ensures expired or
// invalidated tokens don't leave the user in a broken state.
api.interceptors.response.use(
  response => response,
  error => {
    if (error.response?.status === 401) {
      localStorage.removeItem('token')
      localStorage.removeItem('user')
      window.location.href = '/login'
    }
    return Promise.reject(error)
  }
)

// ── AUTH ──────────────────────────────────────────────────────

/** Authenticate a user and receive a JWT token. */
export const login = (email, password) =>
  api.post('/Auth/login', { email, password })

/** Register a new user account. */
export const register = (data) =>
  api.post('/Auth/register', data)

/** Retrieve the authenticated user's profile. */
export const getProfile = () =>
  api.get('/Auth/profile')

// ── FINANCIAL RECORDS ─────────────────────────────────────────

/** Retrieve all financial records for the authenticated user. */
export const getRecords = () =>
  api.get('/FinancialRecords')

/** Retrieve a single financial record by ID. */
export const getRecord = (id) =>
  api.get(`/FinancialRecords/${id}`)

/** Create a new financial record. */
export const createRecord = (data) =>
  api.post('/FinancialRecords', data)

/** Update an existing financial record by ID. */
export const updateRecord = (id, data) =>
  api.put(`/FinancialRecords/${id}`, data)

/** Soft-delete a financial record by ID. */
export const deleteRecord = (id) =>
  api.delete(`/FinancialRecords/${id}`)

/** Search records by keyword across title, description, category, and tags. */
export const searchRecords = (keyword) =>
  api.get('/FinancialRecords/search', { params: { keyword } })

/** Retrieve records filtered by category. */
export const getRecordsByCategory = (category) =>
  api.get(`/FinancialRecords/category/${category}`)

/** Retrieve records within a date range. */
export const getRecordsByDateRange = (startDate, endDate) =>
  api.get('/FinancialRecords/daterange', { params: { startDate, endDate } })

// ── REPORTS ───────────────────────────────────────────────────

/** Retrieve all generated reports for the authenticated user. */
export const getReports = () =>
  api.get('/Reports')

/** Generate a PDF financial report for the specified date range. */
export const generatePdfReport = (title, startDate, endDate) =>
  api.post('/Reports/pdf', null, { params: { title, startDate, endDate } })

/** Generate an Excel financial report for the specified date range. */
export const generateExcelReport = (title, startDate, endDate) =>
  api.post('/Reports/excel', null, { params: { title, startDate, endDate } })

/** Download a report file by ID as a binary blob. */
export const downloadReport = (id) =>
  api.get(`/Reports/${id}/download`, { responseType: 'blob' })

/** Delete a report by ID. */
export const deleteReport = (id) =>
  api.delete(`/Reports/${id}`)

export default api
