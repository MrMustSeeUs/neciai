/*
 * File:    ReportService.cs
 * Purpose: Implements IReportService providing PDF and Excel report
 *          generation for NeciAI financial data.
 *          Satisfies rubric requirements for reports with:
 *          - A title
 *          - Multiple columns
 *          - Multiple rows
 *          - Date-time stamps
 *          Uses iText7 for PDF and EPPlus for Excel generation.
 * Author:  Abraham Macias
 * Date:    June 2026
 * Dependencies: iText7, EPPlus, Entity Framework Core
 */

using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NeciAI.API.Data;
using NeciAI.API.Interfaces;
using NeciAI.API.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NeciAI.API.Services
{
    /// <summary>
    /// Concrete implementation of IReportService.
    /// Generates professional PDF and Excel financial reports
    /// with titles, multiple columns, multiple rows, and timestamps.
    /// </summary>
    public class ReportService : IReportService
    {
        private readonly NeciAIDbContext _context;
        private readonly IFinancialService _financialService;
        private readonly string _storagePath;

        public ReportService(
            NeciAIDbContext context,
            IFinancialService financialService,
            IConfiguration configuration)
        {
            _context = context;
            _financialService = financialService;
            _storagePath = configuration["ReportSettings:StoragePath"] ?? "Reports";

            if (!Directory.Exists(_storagePath))
                Directory.CreateDirectory(_storagePath);
        }

        /// <summary>
        /// Generates a PDF financial report with title, date-time stamp,
        /// and a table containing multiple columns and rows of financial data.
        /// </summary>
        public async Task<Report> GeneratePdfReportAsync(
            string userId, string title, DateTime startDate, DateTime endDate)
        {
            // Ensure UTC kind for all DateTime values saved to PostgreSQL
            var startUtc = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var endUtc = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
            var now = DateTime.UtcNow;

            var records = (await _financialService
                .GetByDateRangeAsync(startUtc, endUtc, userId)).ToList();

            var fileName = $"Report_{userId}_{now:yyyyMMddHHmmss}.pdf";
            var filePath = Path.Combine(_storagePath, fileName);

            using var writer = new PdfWriter(filePath);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            document.Add(new Paragraph(title)
                .SetFontSize(20)
                .SetBold()
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph(
                $"Generated: {now:yyyy-MM-dd HH:mm:ss} UTC | " +
                $"Period: {startUtc:yyyy-MM-dd} to {endUtc:yyyy-MM-dd}")
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph("\n"));

            var totalRevenue = records
                .Where(r => r.Category == "Revenue").Sum(r => r.Amount);
            var totalExpenses = records
                .Where(r => r.Category == "Expense").Sum(r => r.Amount);
            var netPosition = totalRevenue - totalExpenses;

            document.Add(new Paragraph(
                $"Total Revenue: ${totalRevenue:F2}  |  " +
                $"Total Expenses: ${totalExpenses:F2}  |  " +
                $"Net Position: ${netPosition:F2}")
                .SetFontSize(11)
                .SetBold());

            document.Add(new Paragraph("\n"));

            var table = new Table(new float[] { 2, 4, 2, 2, 2 })
                .UseAllAvailableWidth();

            string[] headers = { "Date", "Title", "Category", "Amount", "Anomaly" };
            foreach (var header in headers)
            {
                table.AddHeaderCell(new Cell()
                    .Add(new Paragraph(header).SetBold())
                    .SetBackgroundColor(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY));
            }

            foreach (var record in records)
            {
                table.AddCell(record.RecordDate.ToString("yyyy-MM-dd"));
                table.AddCell(record.Title);
                table.AddCell(record.Category);
                table.AddCell($"${record.Amount:F2}");
                table.AddCell(record.IsAnomaly ? "Yes" : "No");
            }

            document.Add(table);

            var report = new Report
            {
                Title = title,
                ReportType = "Financial Summary",
                Format = "PDF",
                PeriodStart = startUtc,
                PeriodEnd = endUtc,
                GeneratedAt = now,
                FilePath = filePath,
                RowCount = records.Count,
                Summary = $"Revenue: ${totalRevenue:F2} | " +
                          $"Expenses: ${totalExpenses:F2} | " +
                          $"Net: ${netPosition:F2}",
                UserId = userId
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
            return report;
        }

        /// <summary>
        /// Generates an Excel financial report with title, timestamp,
        /// multiple columns, and multiple rows of financial data.
        /// </summary>
        public async Task<Report> GenerateExcelReportAsync(
            string userId, string title, DateTime startDate, DateTime endDate)
        {
            // Ensure UTC kind for all DateTime values saved to PostgreSQL
            var startUtc = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var endUtc = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
            var now = DateTime.UtcNow;

            var records = (await _financialService
                .GetByDateRangeAsync(startUtc, endUtc, userId)).ToList();

            var fileName = $"Report_{userId}_{now:yyyyMMddHHmmss}.xlsx";
            var filePath = Path.Combine(_storagePath, fileName);

            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Financial Report");

            sheet.Cells["A1:F1"].Merge = true;
            sheet.Cells["A1"].Value = title;
            sheet.Cells["A1"].Style.Font.Size = 18;
            sheet.Cells["A1"].Style.Font.Bold = true;
            sheet.Cells["A1"].Style.HorizontalAlignment =
                ExcelHorizontalAlignment.Center;

            sheet.Cells["A2:F2"].Merge = true;
            sheet.Cells["A2"].Value =
                $"Generated: {now:yyyy-MM-dd HH:mm:ss} UTC | " +
                $"Period: {startUtc:yyyy-MM-dd} to {endUtc:yyyy-MM-dd}";
            sheet.Cells["A2"].Style.HorizontalAlignment =
                ExcelHorizontalAlignment.Center;

            var headers = new[]
            {
                "Date", "Title", "Category", "Amount", "Anomaly", "Tags"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                sheet.Cells[4, i + 1].Value = headers[i];
                sheet.Cells[4, i + 1].Style.Font.Bold = true;
                sheet.Cells[4, i + 1].Style.Fill.PatternType =
                    ExcelFillStyle.Solid;
                sheet.Cells[4, i + 1].Style.Fill.BackgroundColor
                    .SetColor(System.Drawing.Color.FromArgb(31, 56, 100));
                sheet.Cells[4, i + 1].Style.Font.Color
                    .SetColor(System.Drawing.Color.White);
            }

            int row = 5;
            foreach (var record in records)
            {
                sheet.Cells[row, 1].Value = record.RecordDate.ToString("yyyy-MM-dd");
                sheet.Cells[row, 2].Value = record.Title;
                sheet.Cells[row, 3].Value = record.Category;
                sheet.Cells[row, 4].Value = record.Amount;
                sheet.Cells[row, 4].Style.Numberformat.Format = "$#,##0.00";
                sheet.Cells[row, 5].Value = record.IsAnomaly ? "Yes" : "No";
                sheet.Cells[row, 6].Value = record.Tags;

                if (record.IsAnomaly)
                {
                    sheet.Cells[row, 1, row, 6].Style.Fill.PatternType =
                        ExcelFillStyle.Solid;
                    sheet.Cells[row, 1, row, 6].Style.Fill.BackgroundColor
                        .SetColor(System.Drawing.Color.FromArgb(255, 220, 220));
                }

                row++;
            }

            sheet.Cells[row + 1, 1].Value = "TOTALS";
            sheet.Cells[row + 1, 1].Style.Font.Bold = true;
            sheet.Cells[row + 1, 4].Formula =
                $"=SUMIF(C5:C{row - 1},\"Revenue\",D5:D{row - 1})";
            sheet.Cells[row + 2, 1].Value = "Net Position";
            sheet.Cells[row + 2, 1].Style.Font.Bold = true;

            sheet.Cells.AutoFitColumns();
            await package.SaveAsAsync(new FileInfo(filePath));

            var totalRevenue = records
                .Where(r => r.Category == "Revenue").Sum(r => r.Amount);
            var totalExpenses = records
                .Where(r => r.Category == "Expense").Sum(r => r.Amount);

            var report = new Report
            {
                Title = title,
                ReportType = "Financial Summary",
                Format = "Excel",
                PeriodStart = startUtc,
                PeriodEnd = endUtc,
                GeneratedAt = now,
                FilePath = filePath,
                RowCount = records.Count,
                Summary = $"Revenue: ${totalRevenue:F2} | " +
                          $"Expenses: ${totalExpenses:F2}",
                UserId = userId
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
            return report;
        }

        /// <summary>
        /// Retrieves all reports generated by a specific user.
        /// </summary>
        public async Task<IEnumerable<Report>> GetReportsByUserAsync(string userId)
        {
            return await _context.Reports
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.GeneratedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves a single report by ID for the authenticated user.
        /// </summary>
        public async Task<Report?> GetReportByIdAsync(int id, string userId)
        {
            return await _context.Reports
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
        }

        /// <summary>
        /// Soft deletes a report record and removes its file from disk.
        /// </summary>
        public async Task<bool> DeleteReportAsync(int id, string userId)
        {
            var report = await _context.Reports
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (report == null) return false;

            if (File.Exists(report.FilePath))
                File.Delete(report.FilePath);

            report.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Reads the report file from disk and returns it as a byte array.
        /// </summary>
        public async Task<byte[]?> DownloadReportAsync(int id, string userId)
        {
            var report = await _context.Reports
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (report == null || !File.Exists(report.FilePath))
                return null;

            return await File.ReadAllBytesAsync(report.FilePath);
        }
    }
}