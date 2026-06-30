// ============================================================
// FinancialRecordsController.cs
// Handles CRUD operations, search, category filtering,
// and date range filtering for financial records.
//
// Author: Abraham Macias
// Route:  /api/FinancialRecords
// ============================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NeciAI.API.DTOs;
using NeciAI.API.Interfaces;
using NeciAI.API.Models;
using System.Security.Claims;

namespace NeciAI.API.Controllers
{
    /// <summary>
    /// Provides full CRUD access to the authenticated user's financial records.
    /// All endpoints require a valid JWT Bearer token.
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
        /// Returns all financial records belonging to the authenticated user,
        /// ordered by record date descending.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var records = await _financialService.GetAllByUserAsync(GetUserId());
            return Ok(records);
        }

        /// <summary>
        /// Returns a single financial record by ID.
        /// Returns 404 if the record does not exist or belongs to another user.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var record = await _financialService.GetByIdAsync(id, GetUserId());
            return record is null
                ? NotFound(new { message = "Record not found." })
                : Ok(record);
        }

        /// <summary>
        /// Creates a new financial record for the authenticated user.
        /// The record is associated with the user's ID from the JWT claim.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] FinancialRecordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var record = MapDtoToRecord(dto, GetUserId());
            var created = await _financialService.CreateAsync(record);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Updates an existing financial record.
        /// Returns 404 if the record does not exist or belongs to another user.
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] FinancialRecordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var record  = MapDtoToRecord(dto, GetUserId());
            var updated = await _financialService.UpdateAsync(id, record, GetUserId());

            return updated is null
                ? NotFound(new { message = "Record not found." })
                : Ok(updated);
        }

        /// <summary>
        /// Soft-deletes a financial record by setting IsDeleted = true.
        /// Records are never permanently removed to maintain audit trails.
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _financialService.DeleteAsync(id, GetUserId());
            return success
                ? Ok(new { message = "Record deleted successfully." })
                : NotFound(new { message = "Record not found." });
        }

        /// <summary>
        /// Searches financial records by keyword across title, description,
        /// category, and tags. Returns multiple rows for all matching records.
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword) || keyword.Trim().Length < 2)
                return BadRequest(new { message = "Search keyword must be at least 2 characters." });

            var results = await _financialService.SearchAsync(keyword, GetUserId());
            return Ok(new { keyword, results });
        }

        /// <summary>
        /// Returns all records for the authenticated user in the specified category.
        /// </summary>
        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetByCategory(string category)
        {
            var records = await _financialService.GetByCategoryAsync(category, GetUserId());
            return Ok(records);
        }

        /// <summary>
        /// Returns all records within the specified date range, ordered by date ascending.
        /// Used by the report generation service to gather data sets.
        /// </summary>
        [HttpGet("daterange")]
        public async Task<IActionResult> GetByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            if (startDate > endDate)
                return BadRequest(new { message = "Start date must be before end date." });

            var records = await _financialService.GetByDateRangeAsync(startDate, endDate, GetUserId());
            return Ok(records);
        }

        // ── PRIVATE HELPERS ──────────────────────────────────

        /// <summary>
        /// Extracts the authenticated user's ID from the JWT claims.
        /// </summary>
        private string GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User identity claim not found.");

        /// <summary>
        /// Maps a FinancialRecordDto to a FinancialRecord model,
        /// associating it with the authenticated user's ID.
        /// </summary>
        private static FinancialRecord MapDtoToRecord(FinancialRecordDto dto, string userId) =>
            new()
            {
                Title       = dto.Title,
                Description = dto.Description ?? string.Empty,
                Category    = dto.Category,
                Amount      = dto.Amount,
                RecordDate  = dto.RecordDate,
                Tags        = dto.Tags ?? string.Empty,
                UserId      = userId,
            };
    }
}
