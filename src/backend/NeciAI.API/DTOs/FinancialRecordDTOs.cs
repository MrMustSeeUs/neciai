/*
 * File:    FinancialRecordDTOs.cs
 * Purpose: Data Transfer Objects for financial record endpoints.
 *          Separates API input/output from internal model structure.
 *          Validation attributes enforce data integrity at the
 *          API boundary before reaching the service layer.
 * Author:  Abraham Macias
 * Date:    June 2026
 */

using System;
using System.ComponentModel.DataAnnotations;

namespace NeciAI.API.DTOs
{
    /// <summary>
    /// Data received when creating or updating a financial record.
    /// All validation happens here before touching the database.
    /// </summary>
    public class FinancialRecordDto
    {
        [Required(ErrorMessage = "Title is required.")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required.")]
        [RegularExpression("^(Budget|Expense|Revenue)$",
            ErrorMessage = "Category must be Budget, Expense, or Revenue.")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, double.MaxValue,
            ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Record date is required.")]
        public DateTime RecordDate { get; set; }

        public string Tags { get; set; } = string.Empty;
    }

    /// <summary>
    /// Data returned when fetching financial records.
    /// Includes computed fields and anomaly detection results.
    /// </summary>
    public class FinancialRecordResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime RecordDate { get; set; }
        public string Tags { get; set; } = string.Empty;
        public bool IsAnomaly { get; set; }
        public decimal AnomalyScore { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Summary { get; set; } = string.Empty;
    }

    /// <summary>
    /// Parameters for searching financial records.
    /// </summary>
    public class SearchDto
    {
        [Required(ErrorMessage = "Search keyword is required.")]
        [MinLength(2, ErrorMessage = "Search term must be at least 2 characters.")]
        public string Keyword { get; set; } = string.Empty;
    }
}