/*
 * File:    api.js
 * Purpose: Centralized API service for all HTTP requests to the
 *          NeciAI C# backend. Uses axios with automatic JWT token
 *          injection on every request via an interceptor.
 *          All API calls go through this single service layer.
 * Author:  Abraham Macias
 * Date:    June 2026
 */

import axios from 'axios'

// Base axios instance pointing to our C# API
// In development the Vite proxy routes /api to localhost:5005
const api = axios.create({
    baseURL: import.meta.env.VITE_API_URL ? `${import.meta.env.VITE_API_URL}/api` : '/api',
    headers: { 'Content-Type': 'application/json' },
})

// ─────────────────────────────────────────────
// REQUEST INTERCEPTOR
// Automatically attaches the JWT token to every
// outgoing request so we never forget to include it
// ─────────────────────────────────────────────
api.interceptors.request.use((config) => {
    const token = localStorage.getItem('neciai_token')
    if (token) {
        config.headers.Authorization = `Bearer ${token}`
    }
    return config
})

// ─────────────────────────────────────────────
// RESPONSE INTERCEPTOR
// Automatically redirects to login if token expires
// ─────────────────────────────────────────────
api.interceptors.response.use(
    (response) => response,
    (error) => {
        if (error.response?.status === 401) {
            localStorage.removeItem('neciai_token')
            localStorage.removeItem('neciai_user')
            window.location.href = '/login'
        }
        return Promise.reject(error)
    }
)

// ─────────────────────────────────────────────
// AUTH ENDPOINTS
// ─────────────────────────────────────────────

/** Register a new user account */
export const register = (data) => api.post('/Auth/register', data)

/** Login and receive a JWT token */
export const login = (data) => api.post('/Auth/login', data)

/** Get the authenticated user's profile */
export const getProfile = () => api.get('/Auth/profile')

// ─────────────────────────────────────────────
// FINANCIAL RECORD ENDPOINTS
// ─────────────────────────────────────────────

/** Get all financial records for the current user */
export const getRecords = () => api.get('/FinancialRecords')

/** Get a single record by ID */
export const getRecord = (id) => api.get(`/FinancialRecords/${id}`)

/** Search records by keyword — returns multiple rows */
export const searchRecords = (keyword) =>
    api.get(`/FinancialRecords/search?keyword=${encodeURIComponent(keyword)}`)

/** Get records filtered by category */
export const getRecordsByCategory = (category) =>
    api.get(`/FinancialRecords/category/${category}`)

/** Get records within a date range */
export const getRecordsByDateRange = (start, end) =>
    api.get(`/FinancialRecords/daterange?start=${start}&end=${end}`)

/** Create a new financial record */
export const createRecord = (data) => api.post('/FinancialRecords', data)

/** Update an existing financial record */
export const updateRecord = (id, data) =>
    api.put(`/FinancialRecords/${id}`, data)

/** Soft delete a financial record */
export const deleteRecord = (id) => api.delete(`/FinancialRecords/${id}`)

// ─────────────────────────────────────────────
// REPORT ENDPOINTS
// ─────────────────────────────────────────────

/** Get all reports for the current user */
export const getReports = () => api.get('/Reports')

/** Generate a PDF report */
export const generatePdfReport = (title, startDate, endDate) =>
    api.post(`/Reports/pdf?title=${encodeURIComponent(title)}&startDate=${startDate}&endDate=${endDate}`)

/** Generate an Excel report */
export const generateExcelReport = (title, startDate, endDate) =>
    api.post(`/Reports/excel?title=${encodeURIComponent(title)}&startDate=${startDate}&endDate=${endDate}`)

/** Download a report file */
export const downloadReport = (id) =>
    api.get(`/Reports/${id}/download`, { responseType: 'blob' })

/** Delete a report */
export const deleteReport = (id) => api.delete(`/Reports/${id}`)

export default api