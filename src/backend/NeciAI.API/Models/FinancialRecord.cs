/*
 * File:    FinancialRecord.cs
 * Purpose: Represents a financial data entry (budget, expense, or revenue).
 *          Inherits from BaseEntity demonstrating INHERITANCE.
 *          Overrides GetSummary() demonstrating POLYMORPHISM.
 *          Uses private backing fields to demonstrate ENCAPSULATION.
 * Author:  Abraham Macias
 * Date:    June 2026
 */

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeciAI.API.Models
{
    /// <summary>
    /// Represents a single financial record in the NeciAI system.
    /// Inherits shared audit fields from BaseEntity.
    /// Supports three categories: Budget, Expense, and Revenue.
    /// </summary>
    public class FinancialRecord : BaseEntity
    {
        // Private backing field — demonstrates ENCAPSULATION
        // Amount cannot be set to a negative value
        private decimal _amount;

        // Title of the financial record — required field with max length
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        // Description providing additional context for this record
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        // Category: Budget, Expense, or Revenue
        [Required]
        public string Category { get; set; } = string.Empty;

        // Encapsulated Amount property — validates business rule
        // that financial amounts cannot be negative
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount
        {
            get => _amount;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Amount cannot be negative.");
                _amount = value;
            }
        }

        // The date this financial event occurred
        [Required]
        public DateTime RecordDate { get; set; }

        // Tags for filtering and search — stored as comma-separated values
        public string Tags { get; set; } = string.Empty;

        // Whether an anomaly was detected in this record by the AI engine
        public bool IsAnomaly { get; set; } = false;

        // Anomaly confidence score returned by the Python microservice
        [Column(TypeName = "decimal(5,4)")]
        public decimal AnomalyScore { get; set; } = 0;

        // Foreign key linking this record to its owner
        public string UserId { get; set; } = string.Empty;

        // Navigation property back to the user who owns this record
        public virtual ApplicationUser User { get; set; } = null!;

        /// <summary>
        /// Overrides BaseEntity.GetSummary() to include financial details.
        /// This is POLYMORPHISM — same method name, different behavior
        /// depending on which class you call it on.
        /// </summary>
        public override string GetSummary()
        {
            return $"[{Category}] {Title}: ${Amount:F2} on {RecordDate:yyyy-MM-dd}";
        }

        /// <summary>
        /// Overrides BaseEntity.OnBeforeSave() to normalize data before
        /// saving to the database. Demonstrates POLYMORPHISM.
        /// </summary>
        public override void OnBeforeSave()
        {
            base.OnBeforeSave(); // Call parent method first
            Title = Title.Trim();
            Description = Description.Trim();
            Category = Category.Trim();
        }
    }
}