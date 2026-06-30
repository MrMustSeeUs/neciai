/*
 * File:    FinancialRecordTests.cs
 * Purpose: Unit tests for the FinancialRecord model and
 *          FinancialService CRUD operations.
 *          Tests cover: model validation, OOP behavior,
 *          CRUD operations, and search functionality.
 * Author:  Abraham Macias
 * Date:    June 2026
 * Dependencies: xUnit, FluentAssertions, EF Core InMemory
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NeciAI.API.Data;
using NeciAI.API.Models;
using NeciAI.API.Services;
using System;
using System.Threading.Tasks;
using Xunit;

namespace NeciAI.Tests
{
    /// <summary>
    /// Unit tests for FinancialRecord model and FinancialService.
    /// Uses an in-memory database to isolate tests from production data.
    /// </summary>
    public class FinancialRecordTests : IDisposable
    {
        private readonly NeciAIDbContext _context;
        private readonly FinancialService _service;
        private const string TestUserId = "test-user-001";

        /// <summary>
        /// Constructor sets up a fresh in-memory database for each test.
        /// This ensures tests are isolated and don't affect each other.
        /// </summary>
        public FinancialRecordTests()
        {
            var options = new DbContextOptionsBuilder<NeciAIDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new NeciAIDbContext(options);
            _service = new FinancialService(_context);
        }

        // ─────────────────────────────────────────────
        // MODEL TESTS — Verify OOP behavior
        // ─────────────────────────────────────────────

        /// <summary>
        /// Test 1: Verifies ENCAPSULATION — Amount cannot be set negative.
        /// The private backing field enforces this business rule.
        /// </summary>
        [Fact]
        public void FinancialRecord_Amount_ThrowsException_WhenNegative()
        {
            // Arrange
            var record = new FinancialRecord();

            // Act & Assert
            var act = () => record.Amount = -100;
            act.Should().Throw<ArgumentException>()
               .WithMessage("*Amount cannot be negative*");
        }

        /// <summary>
        /// Test 2: Verifies ENCAPSULATION — Amount accepts valid positive values.
        /// </summary>
        [Fact]
        public void FinancialRecord_Amount_AcceptsPositiveValue()
        {
            // Arrange
            var record = new FinancialRecord();

            // Act
            record.Amount = 5000.00m;

            // Assert
            record.Amount.Should().Be(5000.00m);
        }

        /// <summary>
        /// Test 3: Verifies POLYMORPHISM — GetSummary() returns
        /// FinancialRecord-specific output, not BaseEntity's default.
        /// </summary>
        [Fact]
        public void FinancialRecord_GetSummary_ReturnsCorrectFormat()
        {
            // Arrange
            var record = new FinancialRecord
            {
                Title = "Q2 Revenue",
                Category = "Revenue",
                Amount = 125000.00m,
                RecordDate = new DateTime(2026, 6, 15)
            };

            // Act
            var summary = record.GetSummary();

            // Assert
            summary.Should().Contain("Revenue");
            summary.Should().Contain("Q2 Revenue");
            summary.Should().Contain("125000");
            summary.Should().Contain("2026-06-15");
        }

        /// <summary>
        /// Test 4: Verifies POLYMORPHISM — Report.GetSummary() returns
        /// different output than FinancialRecord.GetSummary().
        /// Same method name, different behavior = polymorphism.
        /// </summary>
        [Fact]
        public void Report_GetSummary_ReturnsDifferentFormatThanFinancialRecord()
        {
            // Arrange
            var record = new FinancialRecord
            {
                Title = "Test Record",
                Category = "Expense",
                Amount = 500m,
                RecordDate = DateTime.Now
            };

            var report = new Report
            {
                Title = "Test Report",
                ReportType = "Financial Summary",
                Format = "PDF",
                PeriodStart = DateTime.Now.AddDays(-30),
                PeriodEnd = DateTime.Now,
                RowCount = 10
            };

            // Act
            var recordSummary = record.GetSummary();
            var reportSummary = report.GetSummary();

            // Assert — both return summaries but in different formats
            recordSummary.Should().NotBe(reportSummary);
            recordSummary.Should().Contain("Expense");
            reportSummary.Should().Contain("Financial Summary");
            reportSummary.Should().Contain("Rows: 10");
        }

        /// <summary>
        /// Test 5: Verifies INHERITANCE — FinancialRecord inherits
        /// BaseEntity fields (Id, CreatedAt, UpdatedAt, IsDeleted).
        /// </summary>
        [Fact]
        public void FinancialRecord_InheritsBaseEntity_Fields()
        {
            // Arrange & Act
            var record = new FinancialRecord();

            // Assert — these properties come from BaseEntity via inheritance
            record.Should().BeAssignableTo<BaseEntity>();
            record.IsDeleted.Should().BeFalse();
            record.CreatedAt.Should().BeCloseTo(DateTime.UtcNow,
                precision: TimeSpan.FromSeconds(5));
        }

        /// <summary>
        /// Test 6: Verifies INHERITANCE — Report also inherits BaseEntity.
        /// Demonstrates that multiple classes inherit from the same base.
        /// </summary>
        [Fact]
        public void Report_InheritsBaseEntity_Fields()
        {
            // Arrange & Act
            var report = new Report();

            // Assert
            report.Should().BeAssignableTo<BaseEntity>();
            report.IsDeleted.Should().BeFalse();
        }

        // ─────────────────────────────────────────────
        // SERVICE TESTS — Verify CRUD operations
        // ─────────────────────────────────────────────

        /// <summary>
        /// Test 7: Verifies CREATE — A new financial record can be
        /// added to the database and retrieved with correct data.
        /// </summary>
        [Fact]
        public async Task CreateAsync_CreatesRecord_Successfully()
        {
            // Arrange
            var record = new FinancialRecord
            {
                Title = "Office Supplies",
                Description = "Monthly office supply purchase",
                Category = "Expense",
                Amount = 250.00m,
                RecordDate = new DateTime(2026, 6, 1),
                Tags = "Office,Monthly",
                UserId = TestUserId
            };

            // Act
            var created = await _service.CreateAsync(record);

            // Assert
            created.Should().NotBeNull();
            created.Id.Should().BeGreaterThan(0);
            created.Title.Should().Be("Office Supplies");
            created.Amount.Should().Be(250.00m);
            created.Category.Should().Be("Expense");
            created.UserId.Should().Be(TestUserId);
        }

        /// <summary>
        /// Test 8: Verifies READ — All records for a user can be retrieved.
        /// </summary>
        [Fact]
        public async Task GetAllByUserAsync_ReturnsAllUserRecords()
        {
            // Arrange — create three records for our test user
            await _service.CreateAsync(new FinancialRecord
            {
                Title = "Revenue January",
                Category = "Revenue",
                Amount = 10000m,
                RecordDate = new DateTime(2026, 1, 1),
                UserId = TestUserId
            });

            await _service.CreateAsync(new FinancialRecord
            {
                Title = "Expense January",
                Category = "Expense",
                Amount = 3000m,
                RecordDate = new DateTime(2026, 1, 15),
                UserId = TestUserId
            });

            await _service.CreateAsync(new FinancialRecord
            {
                Title = "Other User Record",
                Category = "Revenue",
                Amount = 5000m,
                RecordDate = new DateTime(2026, 1, 20),
                UserId = "other-user-999" // Different user
            });

            // Act
            var results = await _service.GetAllByUserAsync(TestUserId);

            // Assert — should only return our test user's 2 records
            results.Should().HaveCount(2);
            results.Should().AllSatisfy(r => r.UserId.Should().Be(TestUserId));
        }

        /// <summary>
        /// Test 9: Verifies UPDATE — An existing record can be modified.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_UpdatesRecord_Successfully()
        {
            // Arrange
            var original = await _service.CreateAsync(new FinancialRecord
            {
                Title = "Original Title",
                Category = "Expense",
                Amount = 100m,
                RecordDate = new DateTime(2026, 6, 1),
                UserId = TestUserId
            });

            var updated = new FinancialRecord
            {
                Title = "Updated Title",
                Category = "Revenue",
                Amount = 999m,
                RecordDate = new DateTime(2026, 6, 15),
                Tags = "Updated"
            };

            // Act
            var result = await _service.UpdateAsync(original.Id, updated, TestUserId);

            // Assert
            result.Should().NotBeNull();
            result!.Title.Should().Be("Updated Title");
            result.Amount.Should().Be(999m);
            result.Category.Should().Be("Revenue");
        }

        /// <summary>
        /// Test 10: Verifies DELETE — A record is soft-deleted
        /// (IsDeleted = true) rather than permanently removed.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_SoftDeletesRecord_Successfully()
        {
            // Arrange
            var record = await _service.CreateAsync(new FinancialRecord
            {
                Title = "Record To Delete",
                Category = "Expense",
                Amount = 50m,
                RecordDate = DateTime.Now,
                UserId = TestUserId
            });

            // Act
            var result = await _service.DeleteAsync(record.Id, TestUserId);

            // Assert
            result.Should().BeTrue();

            // Verify it no longer appears in queries (global filter hides it)
            var records = await _service.GetAllByUserAsync(TestUserId);
            records.Should().BeEmpty();
        }

        /// <summary>
        /// Test 11: Verifies SEARCH — Search returns multiple rows
        /// matching keyword across title, description, category, tags.
        /// Satisfies the rubric search requirement.
        /// </summary>
        [Fact]
        public async Task SearchAsync_ReturnsMultipleMatchingRecords()
        {
            // Arrange — create records with searchable content
            await _service.CreateAsync(new FinancialRecord
            {
                Title = "Meridian Client Revenue",
                Category = "Revenue",
                Amount = 50000m,
                RecordDate = new DateTime(2026, 1, 1),
                Tags = "Meridian,Q1",
                UserId = TestUserId
            });

            await _service.CreateAsync(new FinancialRecord
            {
                Title = "Meridian Consulting Fee",
                Category = "Revenue",
                Amount = 15000m,
                RecordDate = new DateTime(2026, 2, 1),
                Tags = "Meridian,Consulting",
                UserId = TestUserId
            });

            await _service.CreateAsync(new FinancialRecord
            {
                Title = "Office Rent",
                Category = "Expense",
                Amount = 3000m,
                RecordDate = new DateTime(2026, 1, 1),
                UserId = TestUserId
            });

            // Act — search for "Meridian" should return 2 records
            var results = await _service.SearchAsync("Meridian", TestUserId);

            // Assert — multiple rows returned
            results.Should().HaveCount(2);
            results.Should().AllSatisfy(r =>
                (r.Title.ToLower().Contains("meridian") ||
                 r.Tags.ToLower().Contains("meridian")).Should().BeTrue());
        }

        /// <summary>
        /// Test 12: Verifies VALIDATION — GetById returns null
        /// when record belongs to a different user (security check).
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenUserUnauthorized()
        {
            // Arrange
            var record = await _service.CreateAsync(new FinancialRecord
            {
                Title = "Private Record",
                Category = "Revenue",
                Amount = 10000m,
                RecordDate = DateTime.Now,
                UserId = "owner-user-123"
            });

            // Act — try to access with wrong user ID
            var result = await _service.GetByIdAsync(record.Id, "wrong-user-456");

            // Assert — should return null, not the record
            result.Should().BeNull();
        }

        /// <summary>
        /// Test 13: Verifies DATE RANGE filter returns correct records.
        /// Used by report generation to gather data sets.
        /// </summary>
        [Fact]
        public async Task GetByDateRangeAsync_ReturnsRecordsWithinRange()
        {
            // Arrange
            await _service.CreateAsync(new FinancialRecord
            {
                Title = "January Record",
                Category = "Revenue",
                Amount = 1000m,
                RecordDate = new DateTime(2026, 1, 15),
                UserId = TestUserId
            });

            await _service.CreateAsync(new FinancialRecord
            {
                Title = "March Record",
                Category = "Expense",
                Amount = 500m,
                RecordDate = new DateTime(2026, 3, 15),
                UserId = TestUserId
            });

            await _service.CreateAsync(new FinancialRecord
            {
                Title = "June Record",
                Category = "Revenue",
                Amount = 2000m,
                RecordDate = new DateTime(2026, 6, 15),
                UserId = TestUserId
            });

            // Act — query only Q1 (Jan 1 to Mar 31)
            var results = await _service.GetByDateRangeAsync(
                new DateTime(2026, 1, 1),
                new DateTime(2026, 3, 31),
                TestUserId);

            // Assert — only January and March records returned
            results.Should().HaveCount(2);
            results.Should().NotContain(r => r.Title == "June Record");
        }

        /// <summary>
        /// Test 14: Verifies OnBeforeSave() POLYMORPHISM —
        /// FinancialRecord's override trims whitespace before saving.
        /// </summary>
        [Fact]
        public async Task CreateAsync_TrimsWhitespace_ViaOnBeforeSave()
        {
            // Arrange — title and category with extra whitespace
            var record = new FinancialRecord
            {
                Title = "  Padded Title  ",
                Category = "Revenue",
                Amount = 100m,
                RecordDate = DateTime.Now,
                UserId = TestUserId
            };

            // Act
            var created = await _service.CreateAsync(record);

            // Assert — OnBeforeSave() trimmed the whitespace
            created.Title.Should().Be("Padded Title");
        }

        /// <summary>
        /// Cleanup — dispose the in-memory database after each test.
        /// </summary>
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}