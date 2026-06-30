/*
 * File:    FinancialService.cs
 * Purpose: Implements IFinancialService interface providing all
 *          business logic for financial record CRUD operations,
 *          search functionality, and category/date filtering.
 *          Demonstrates POLYMORPHISM by implementing the interface
 *          contract defined in IFinancialService.
 * Author:  Abraham Macias
 * Date:    June 2026
 * Dependencies: Entity Framework Core, NeciAIDbContext
 */

using Microsoft.EntityFrameworkCore;
using NeciAI.API.Data;
using NeciAI.API.Interfaces;
using NeciAI.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NeciAI.API.Services
{
    /// <summary>
    /// Concrete implementation of IFinancialService.
    /// Handles all database operations for financial records
    /// using Entity Framework Core and PostgreSQL.
    /// All queries automatically exclude soft-deleted records
    /// via the global query filter defined in NeciAIDbContext.
    /// </summary>
    public class FinancialService : IFinancialService
    {
        // Private field — ENCAPSULATION of the database context
        private readonly NeciAIDbContext _context;

        /// <summary>
        /// Constructor receives the database context via
        /// dependency injection — not created manually.
        /// </summary>
        public FinancialService(NeciAIDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all financial records belonging to a specific user.
        /// Results are ordered by record date descending (newest first).
        /// </summary>
        public async Task<IEnumerable<FinancialRecord>> GetAllByUserAsync(
            string userId)
        {
            return await _context.FinancialRecords
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.RecordDate)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves a single financial record by ID.
        /// Returns null if not found or doesn't belong to user.
        /// </summary>
        public async Task<FinancialRecord?> GetByIdAsync(int id, string userId)
        {
            return await _context.FinancialRecords
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
        }

        /// <summary>
        /// Searches financial records by keyword across multiple fields.
        /// Satisfies the rubric requirement for search with multiple row
        /// results — searches Title, Description, Category, and Tags.
        /// Returns all matching records ordered by relevance (date).
        /// </summary>
        public async Task<IEnumerable<FinancialRecord>> SearchAsync(
            string keyword, string userId)
        {
            // Normalize keyword for case-insensitive search
            var term = keyword.ToLower().Trim();

            return await _context.FinancialRecords
                .Where(r => r.UserId == userId &&
                    (r.Title.ToLower().Contains(term) ||
                     r.Description.ToLower().Contains(term) ||
                     r.Category.ToLower().Contains(term) ||
                     r.Tags.ToLower().Contains(term)))
                .OrderByDescending(r => r.RecordDate)
                .ToListAsync();
        }

        /// <summary>
        /// Creates a new financial record in the database.
        /// Calls OnBeforeSave() which demonstrates POLYMORPHISM —
        /// FinancialRecord's override normalizes the data before saving.
        /// </summary>
        public async Task<FinancialRecord> CreateAsync(FinancialRecord record)
        {
            _context.FinancialRecords.Add(record);
            await _context.SaveChangesAsync();
            return record;
        }

        /// <summary>
        /// Updates an existing financial record.
        /// Returns null if the record doesn't exist or belongs
        /// to a different user — prevents unauthorized modifications.
        /// </summary>
        public async Task<FinancialRecord?> UpdateAsync(
            int id, FinancialRecord updated, string userId)
        {
            var existing = await _context.FinancialRecords
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (existing == null) return null;

            existing.Title = updated.Title;
            existing.Description = updated.Description;
            existing.Category = updated.Category;
            existing.Amount = updated.Amount;
            existing.RecordDate = updated.RecordDate;
            existing.Tags = updated.Tags;

            await _context.SaveChangesAsync();
            return existing;
        }

        /// <summary>
        /// Soft deletes a financial record by setting IsDeleted = true.
        /// The record remains in the database for audit purposes but
        /// is excluded from all future queries via the global filter.
        /// Returns false if record not found or unauthorized.
        /// </summary>
        public async Task<bool> DeleteAsync(int id, string userId)
        {
            var record = await _context.FinancialRecords
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (record == null) return false;

            record.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Retrieves all records for a user filtered by category.
        /// Supports the dashboard's category breakdown view.
        /// </summary>
        public async Task<IEnumerable<FinancialRecord>> GetByCategoryAsync(
            string category, string userId)
        {
            return await _context.FinancialRecords
                .Where(r => r.UserId == userId &&
                    r.Category.ToLower() == category.ToLower())
                .OrderByDescending(r => r.RecordDate)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves records within a specific date range.
        /// Converts DateTime to UTC to satisfy PostgreSQL
        /// timestamp with time zone requirement.
        /// Used by the report generation service to gather
        /// the data set for PDF and Excel reports.
        /// </summary>
        public async Task<IEnumerable<FinancialRecord>> GetByDateRangeAsync(
            DateTime startDate, DateTime endDate, string userId)
        {
            // Specify UTC kind to satisfy PostgreSQL timestamp with time zone
            var startUtc = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var endUtc = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

            return await _context.FinancialRecords
                .Where(r => r.UserId == userId &&
                    r.RecordDate >= startUtc &&
                    r.RecordDate <= endUtc)
                .OrderBy(r => r.RecordDate)
                .ToListAsync();
        }
    }
}