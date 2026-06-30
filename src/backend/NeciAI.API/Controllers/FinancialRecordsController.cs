/*
 * File:    FinancialRecordsController.cs
 * Purpose: Handles all CRUD operations and search for financial records.
 *          Satisfies rubric requirements for:
 *          - Secure add, modify, delete (JWT protected)
 *          - Search with multiple row results
 *          - Validation functionality
 * Author:  Abraham Macias
 * Date:    June 2026
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NeciAI.API.DTOs;
using NeciAI.API.Interfaces;
using NeciAI.API.Models;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NeciAI.API.Controllers
{
    /// <summary>
    /// Manages financial record CRUD operations and search.
    /// All endpoints require a valid JWT token (Authorize attribute).
    /// Routes are prefixed with /api/financialrecords
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FinancialRecordsController : ControllerBase
    {
        private readonly IFinancialService _financialService;

        public FinancialRecordsController(IFinancialService financialService)
        {
            _financialService = financialService;
        }

        /// <summary>
        /// GET /api/financialrecords
        /// Returns all financial records for the authenticated user.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = GetUserId();
            var records = await _financialService.GetAllByUserAsync(userId);
            return Ok(records.Select(MapToResponse));
        }

        /// <summary>
        /// GET /api/financialrecords/{id}
        /// Returns a single financial record by ID.
        /// Returns 404 if not found or unauthorized.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = GetUserId();
            var record = await _financialService.GetByIdAsync(id, userId);

            if (record == null)
                return NotFound(new { message = "Record not found." });

            return Ok(MapToResponse(record));
        }

        /// <summary>
        /// GET /api/financialrecords/search?keyword={term}
        /// Searches records by keyword across title, description,
        /// category, and tags. Returns multiple matching rows.
        /// Satisfies the rubric search requirement.
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword) || keyword.Length < 2)
                return BadRequest(new
                {
                    message = "Search keyword must be at least 2 characters."
                });

            var userId = GetUserId();
            var results = await _financialService.SearchAsync(keyword, userId);
            var list = results.ToList();

            return Ok(new
            {
                keyword,
                totalResults = list.Count,
                results = list.Select(MapToResponse)
            });
        }

        /// <summary>
        /// GET /api/financialrecords/category/{category}
        /// Returns all records filtered by category.
        /// </summary>
        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetByCategory(string category)
        {
            var userId = GetUserId();
            var records = await _financialService
                .GetByCategoryAsync(category, userId);
            return Ok(records.Select(MapToResponse));
        }

        /// <summary>
        /// GET /api/financialrecords/daterange?start={date}&end={date}
        /// Returns records within a date range for reporting.
        /// </summary>
        [HttpGet("daterange")]
        public async Task<IActionResult> GetByDateRange(
            [FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            if (start > end)
                return BadRequest(new
                {
                    message = "Start date must be before end date."
                });

            var userId = GetUserId();
            var records = await _financialService
                .GetByDateRangeAsync(start, end, userId);
            return Ok(records.Select(MapToResponse));
        }

        /// <summary>
        /// POST /api/financialrecords
        /// Creates a new financial record.
        /// Validates input via DTO data annotations before processing.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] FinancialRecordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();

            var record = new FinancialRecord
            {
                Title = dto.Title,
                Description = dto.Description,
                Category = dto.Category,
                Amount = dto.Amount,
                RecordDate = dto.RecordDate,
                Tags = dto.Tags,
                UserId = userId
            };

            var created = await _financialService.CreateAsync(record);
            return CreatedAtAction(
                nameof(GetById),
                new { id = created.Id },
                MapToResponse(created));
        }

        /// <summary>
        /// PUT /api/financialrecords/{id}
        /// Updates an existing financial record.
        /// Returns 404 if not found or unauthorized.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id, [FromBody] FinancialRecordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();

            var updated = new FinancialRecord
            {
                Title = dto.Title,
                Description = dto.Description,
                Category = dto.Category,
                Amount = dto.Amount,
                RecordDate = dto.RecordDate,
                Tags = dto.Tags
            };

            var result = await _financialService
                .UpdateAsync(id, updated, userId);

            if (result == null)
                return NotFound(new { message = "Record not found." });

            return Ok(MapToResponse(result));
        }

        /// <summary>
        /// DELETE /api/financialrecords/{id}
        /// Soft deletes a financial record.
        /// Returns 404 if not found or unauthorized.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();
            var success = await _financialService.DeleteAsync(id, userId);

            if (!success)
                return NotFound(new { message = "Record not found." });

            return Ok(new { message = "Record deleted successfully." });
        }

        /// <summary>
        /// Extracts the authenticated user's ID from the JWT token claims.
        /// Private helper — encapsulated within the controller.
        /// </summary>
        private string GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User not authenticated.");

        /// <summary>
        /// Maps a FinancialRecord model to a response DTO.
        /// Calls GetSummary() which demonstrates POLYMORPHISM —
        /// each entity type returns its own summary format.
        /// </summary>
        private static FinancialRecordResponseDto MapToResponse(
            FinancialRecord record) => new()
            {
                Id = record.Id,
                Title = record.Title,
                Description = record.Description,
                Category = record.Category,
                Amount = record.Amount,
                RecordDate = record.RecordDate,
                Tags = record.Tags,
                IsAnomaly = record.IsAnomaly,
                AnomalyScore = record.AnomalyScore,
                CreatedAt = record.CreatedAt,
                UpdatedAt = record.UpdatedAt,
                Summary = record.GetSummary() // POLYMORPHISM in action
            };
    }
}