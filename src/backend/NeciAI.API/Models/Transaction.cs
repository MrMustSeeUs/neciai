/*
 * File:    Transaction.cs
 * Purpose: Represents an individual financial transaction linked to a
 *          FinancialRecord. Inherits from BaseEntity demonstrating
 *          INHERITANCE. Overrides GetSummary() demonstrating POLYMORPHISM.
 * Author:  Abraham Macias
 * Date:    June 2026
 */

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeciAI.API.Models
{
    /// <summary>
    /// Represents a single financial transaction (debit or credit)
    /// that belongs to a FinancialRecord. Provides granular tracking
    /// of individual financial events within a record.
    /// </summary>
    public class Transaction : BaseEntity
    {
        // Transaction description — what this transaction was for
        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        // Transaction amount — positive for credits, negative for debits
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        // Type: Credit or Debit
        [Required]
        public string TransactionType { get; set; } = string.Empty;

        // When this transaction occurred
        [Required]
        public DateTime TransactionDate { get; set; }

        // Reference number for external reconciliation
        [MaxLength(100)]
        public string ReferenceNumber { get; set; } = string.Empty;

        // Foreign key to the parent FinancialRecord
        public int FinancialRecordId { get; set; }

        // Navigation property to parent record
        public virtual FinancialRecord FinancialRecord { get; set; } = null!;

        /// <summary>
        /// Overrides GetSummary() for transaction-specific output.
        /// Demonstrates POLYMORPHISM — three classes, three behaviors,
        /// one method name: GetSummary().
        /// </summary>
        public override string GetSummary()
        {
            string sign = Amount >= 0 ? "+" : "";
            return $"[{TransactionType}] {Description}: {sign}${Amount:F2} " +
                   $"on {TransactionDate:yyyy-MM-dd} | Ref: {ReferenceNumber}";
        }
    }
}