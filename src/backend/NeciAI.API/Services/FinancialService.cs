// ============================================================
// FinancialService.cs
// Business logic layer for financial record operations.
// Implements IFinancialService using EF Core and PostgreSQL.
//
// All queries automatically exclude soft-deleted records via
// the global query filter configured in NeciAIDbContext.
//
// Author: Abraham Macias
// ============================================================

using Microsoft.EntityFrameworkCore;
using NeciAI.API.Data;
using NeciAI.API.Interfaces;
using NeciAI.API.Models;

namespace NeciAI.API.Services
{
    /// <summary>
    /// Concrete implementation of IFinancialService.
    /// Demonstrates POLYMORPHISM by implementing the interface contract.
    /// </summary>
    public class FinancialService : IFinancialService
    {
        // Encapsulated dependency — never exposed directly to callers.
        private readonly NeciAIDbContext _context;

        public FinancialService(NeciAIDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FinancialRecord>> GetAllByUserAsync(string userId)
        {
            return await _context.FinancialRecords
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.RecordDate)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<FinancialRecord?> GetByIdAsync(int id, string userId)
        {
            return await _context.FinancialRecords
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Searches Title, Description, Category, and Tags simultaneously
        /// using case-insensitive comparison. Returns multiple rows when
        /// more than one record matches the keyword.
        /// </remarks>
        public async Task<IEnumerable<FinancialRecord>> SearchAsync(string keyword, string userId)
        {
            var term = keyword.ToLower().Trim();

            return await _context.FinancialRecords
                .Where(r => r.UserId == userId && MatchesKeyword(r, term))
                .OrderByDescending(r => r.RecordDate)
                .ToListAsync();
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Calls OnBeforeSave() before persistence, demonstrating
        /// POLYMORPHISM — the FinancialRecord override trims whitespace
        /// and enforces business rules prior to the database write.
        /// </remarks>
        public async Task<FinancialRecord> CreateAsync(FinancialRecord record)
        {
            _context.FinancialRecords.Add(record);
            await _context.SaveChangesAsync();
            return record;
        }

        /// <inheritdoc/>
        public async Task<FinancialRecord?> UpdateAsync(int id, FinancialRecord updated, string userId)
        {
            var existing = await _context.FinancialRecords
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (existing is null) return null;

            existing.Title       = updated.Title;
            existing.Description = updated.Description;
            existing.Category    = updated.Category;
            existing.Amount      = updated.Amount;
            existing.RecordDate  = updated.RecordDate;
            existing.Tags        = updated.Tags;

            await _context.SaveChangesAsync();
            return existing;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Performs a soft delete by setting IsDeleted = true.
        /// Financial records are never permanently removed to support
        /// audit trails and compliance requirements.
        /// </remarks>
        public async Task<bool> DeleteAsync(int id, string userId)
        {
            var record = await _context.FinancialRecords
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (record is null) return false;

            record.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FinancialRecord>> GetByCategoryAsync(string category, string userId)
        {
            return await _context.FinancialRecords
                .Where(r => r.UserId == userId &&
                            r.Category.ToLower() == category.ToLower())
                .OrderByDescending(r => r.RecordDate)
                .ToListAsync();
        }

        /// <inheritdoc/>
        /// <remarks>
        /// DateTime values are explicitly specified as UTC to satisfy
        /// Npgsql's requirement for timestamp with time zone columns.
        /// </remarks>
        public async Task<IEnumerable<FinancialRecord>> GetByDateRangeAsync(
            DateTime startDate, DateTime endDate, string userId)
        {
            var startUtc = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var endUtc   = DateTime.SpecifyKind(endDate,   DateTimeKind.Utc);

            return await _context.FinancialRecords
                .Where(r => r.UserId == userId &&
                            r.RecordDate >= startUtc &&
                            r.RecordDate <= endUtc)
                .OrderBy(r => r.RecordDate)
                .ToListAsync();
        }

        // ── PRIVATE HELPERS ──────────────────────────────────

        /// <summary>
        /// Returns true if the record contains the search term in any
        /// searchable field. Extracted to keep the query readable.
        /// </summary>
        private static bool MatchesKeyword(FinancialRecord r, string term) =>
            r.Title.ToLower().Contains(term)       ||
            r.Description.ToLower().Contains(term) ||
            r.Category.ToLower().Contains(term)    ||
            r.Tags.ToLower().Contains(term);
    }
}
