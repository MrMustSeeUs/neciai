/*
 * File:    IFinancialService.cs
 * Purpose: Defines the contract for all financial record operations.
 *          Using interfaces enforces consistent behavior across services,
 *          enables dependency injection, and makes unit testing possible
 *          by allowing mock implementations during testing.
 * Author:  Abraham Macias
 * Date:    June 2026
 */

using NeciAI.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NeciAI.API.Interfaces
{
    /// <summary>
    /// Interface defining all financial record operations.
    /// Any class implementing this interface MUST provide concrete
    /// implementations of every method defined here.
    /// This satisfies the POLYMORPHISM requirement — multiple classes
    /// can implement this interface in different ways.
    /// </summary>
    public interface IFinancialService
    {
        // Retrieve all financial records for a specific user
        Task<IEnumerable<FinancialRecord>> GetAllByUserAsync(string userId);

        // Retrieve a single record by its ID
        Task<FinancialRecord?> GetByIdAsync(int id, string userId);

        // Search records by keyword across title, description, and category
        // Returns multiple rows — satisfies the search rubric requirement
        Task<IEnumerable<FinancialRecord>> SearchAsync(string keyword, string userId);

        // Create a new financial record
        Task<FinancialRecord> CreateAsync(FinancialRecord record);

        // Update an existing financial record
        Task<FinancialRecord?> UpdateAsync(int id, FinancialRecord record, string userId);

        // Soft delete a record — marks IsDeleted = true, never hard deletes
        Task<bool> DeleteAsync(int id, string userId);

        // Retrieve records filtered by category
        Task<IEnumerable<FinancialRecord>> GetByCategoryAsync(string category, string userId);

        // Retrieve records within a date range for report generation
        Task<IEnumerable<FinancialRecord>> GetByDateRangeAsync(
            DateTime startDate, DateTime endDate, string userId);
    }
}