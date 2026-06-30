/*
 * File:    NeciAIDbContext.cs
 * Purpose: The main database context for NeciAI. Acts as the bridge
 *          between C# model classes and the PostgreSQL database.
 *          Configures all entity relationships, constraints, and
 *          indexes. Extends IdentityDbContext to include ASP.NET
 *          Identity tables for user authentication.
 * Author:  Abraham Macias
 * Date:    June 2026
 * Dependencies: Entity Framework Core, Npgsql, ASP.NET Identity
 */

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NeciAI.API.Models;
using System.Threading;
using System.Threading.Tasks;

namespace NeciAI.API.Data
{
    /// <summary>
    /// Main database context for NeciAI.
    /// Inherits from IdentityDbContext which automatically creates
    /// all ASP.NET Identity tables (Users, Roles, Claims, etc.)
    /// alongside our custom NeciAI tables.
    /// </summary>
    public class NeciAIDbContext : IdentityDbContext<ApplicationUser>
    {
        // Constructor — receives database options injected by ASP.NET Core
        public NeciAIDbContext(DbContextOptions<NeciAIDbContext> options)
            : base(options)
        {
        }

        // ─────────────────────────────────────────
        // DATABASE TABLES
        // Each DbSet<T> maps to a table in PostgreSQL
        // ─────────────────────────────────────────

        /// <summary>Table storing all financial records (budgets, expenses, revenue)</summary>
        public DbSet<FinancialRecord> FinancialRecords { get; set; }

        /// <summary>Table storing all individual transactions</summary>
        public DbSet<Transaction> Transactions { get; set; }

        /// <summary>Table storing all generated reports</summary>
        public DbSet<Report> Reports { get; set; }

        /// <summary>
        /// Configures entity relationships, constraints, indexes,
        /// and table naming conventions.
        /// Called automatically by Entity Framework when building the schema.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Always call base first — sets up Identity tables
            base.OnModelCreating(modelBuilder);

            // ─────────────────────────────────────────
            // FINANCIAL RECORD CONFIGURATION
            // ─────────────────────────────────────────
            modelBuilder.Entity<FinancialRecord>(entity =>
            {
                entity.ToTable("financial_records");

                // Primary key
                entity.HasKey(e => e.Id);

                // Required fields with max lengths
                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Category)
                    .IsRequired()
                    .HasMaxLength(100);

                // Decimal precision for financial amounts
                entity.Property(e => e.Amount)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.AnomalyScore)
                    .HasColumnType("decimal(5,4)");

                // Default values for audit fields
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("NOW()");

                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("NOW()");

                entity.Property(e => e.IsDeleted)
                    .HasDefaultValue(false);

                // Index on UserId for fast user-specific queries
                entity.HasIndex(e => e.UserId)
                    .HasDatabaseName("idx_financial_records_user_id");

                // Index on Category for fast category filtering
                entity.HasIndex(e => e.Category)
                    .HasDatabaseName("idx_financial_records_category");

                // Index on RecordDate for fast date range queries
                entity.HasIndex(e => e.RecordDate)
                    .HasDatabaseName("idx_financial_records_date");

                // Relationship: FinancialRecord belongs to ApplicationUser
                entity.HasOne(e => e.User)
                    .WithMany(u => u.FinancialRecords)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ─────────────────────────────────────────
            // TRANSACTION CONFIGURATION
            // ─────────────────────────────────────────
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.ToTable("transactions");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.Amount)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.TransactionType)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("NOW()");

                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("NOW()");

                entity.Property(e => e.IsDeleted)
                    .HasDefaultValue(false);

                // Index for fast lookup by parent record
                entity.HasIndex(e => e.FinancialRecordId)
                    .HasDatabaseName("idx_transactions_record_id");

                // Index for date range queries
                entity.HasIndex(e => e.TransactionDate)
                    .HasDatabaseName("idx_transactions_date");

                // Relationship: Transaction belongs to FinancialRecord
                entity.HasOne(e => e.FinancialRecord)
                    .WithMany()
                    .HasForeignKey(e => e.FinancialRecordId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ─────────────────────────────────────────
            // REPORT CONFIGURATION
            // ─────────────────────────────────────────
            modelBuilder.Entity<Report>(entity =>
            {
                entity.ToTable("reports");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(300);

                entity.Property(e => e.ReportType)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Format)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("NOW()");

                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("NOW()");

                entity.Property(e => e.IsDeleted)
                    .HasDefaultValue(false);

                // Index for fast user report lookup
                entity.HasIndex(e => e.UserId)
                    .HasDatabaseName("idx_reports_user_id");

                // Index for date-based report queries
                entity.HasIndex(e => e.GeneratedAt)
                    .HasDatabaseName("idx_reports_generated_at");

                // Relationship: Report belongs to ApplicationUser
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Reports)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ─────────────────────────────────────────
            // GLOBAL QUERY FILTERS
            // Automatically excludes soft-deleted records
            // from ALL queries unless explicitly overridden
            // ─────────────────────────────────────────
            modelBuilder.Entity<FinancialRecord>()
                .HasQueryFilter(e => !e.IsDeleted);

            modelBuilder.Entity<Transaction>()
                .HasQueryFilter(e => !e.IsDeleted);

            modelBuilder.Entity<Report>()
                .HasQueryFilter(e => !e.IsDeleted);
        }

        /// <summary>
        /// Override SaveChangesAsync to automatically call OnBeforeSave()
        /// on every modified entity before persisting to the database.
        /// This ensures UpdatedAt is always current — no manual tracking needed.
        /// </summary>
        public override async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            // Find all modified BaseEntity entries
            var entries = ChangeTracker.Entries<BaseEntity>()
                .Where(e => e.State == EntityState.Modified ||
                            e.State == EntityState.Added);

            // Call OnBeforeSave() on each — this is POLYMORPHISM in action:
            // each entity type runs its own version of OnBeforeSave()
            foreach (var entry in entries)
            {
                entry.Entity.OnBeforeSave();
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}