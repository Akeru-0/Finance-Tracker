using ClosedXML.Excel;
using FinanceTracker.Data;
using FinanceTracker.Models;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace FinanceTracker.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly FinanceDbContext _context;

        public TransactionService(FinanceDbContext context)
        {
            _context = context;
        }

        public async Task<List<Transaction>> GetAllTransactionsAsync()
        {
            return await _context.Transactions
                .OrderByDescending(t => t.Date)
                .ToListAsync();
        }

        public async Task<List<Transaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Transactions
                .Where(t => t.Date >= startDate && t.Date <= endDate)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
        }

        public async Task<List<Transaction>> GetTransactionsByCategoryAsync(string category)
        {
            return await _context.Transactions
                .Where(t => t.Category == category)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
        }

        public async Task<List<Transaction>> GetTransactionsByTypeAsync(TransactionType type)
        {
            return await _context.Transactions
                .Where(t => t.Type == type)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
        }

        public async Task<Transaction?> GetTransactionByIdAsync(int id)
        {
            return await _context.Transactions.FindAsync(id);
        }

        public async Task<bool> AddTransactionAsync(Transaction transaction)
        {
            try
            {
                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateTransactionAsync(Transaction transaction)
        {
            try
            {
                _context.Entry(transaction).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteTransactionAsync(int id)
        {
            try
            {
                var transaction = await _context.Transactions.FindAsync(id);
                if (transaction == null) return false;
                
                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<decimal> GetBalanceAsync()
        {
            var income = (await _context.Transactions
                .Where(t => t.Type == TransactionType.Income)
                .ToListAsync())
                .Sum(t => t.Amount);

            var expenses = (await _context.Transactions
                .Where(t => t.Type == TransactionType.Expense)
                .ToListAsync())
                .Sum(t => t.Amount);

            return income - expenses;
        }

        public async Task<Dictionary<string, decimal>> GetCategoryTotalsAsync(
            TransactionType type, DateTime startDate, DateTime endDate)
        {
            var transactions = await _context.Transactions
                .Where(t => t.Type == type && t.Date >= startDate && t.Date <= endDate)
                .ToListAsync();

            return transactions
                .GroupBy(t => t.Category)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(t => t.Amount)
                );
        }

        public async Task<List<KeyValuePair<DateTime, decimal>>> GetDailyTotalsAsync(
            DateTime startDate, DateTime endDate, TransactionType type)
        {
            var transactions = await _context.Transactions
                .Where(t => t.Type == type && t.Date >= startDate && t.Date <= endDate)
                .ToListAsync();

            return transactions
                .GroupBy(t => t.Date.Date)
                .Select(g => new KeyValuePair<DateTime, decimal>(
                    g.Key,
                    g.Sum(t => t.Amount)))
                .ToList();
        }

        public async Task ExportToExcelAsync(string filePath, DateTime startDate, DateTime endDate)
        {
            try
            {
                var transactions = await GetTransactionsByDateRangeAsync(startDate, endDate);
                Debug.WriteLine($"Retrieved {transactions.Count} transactions for Excel export");

                // Create directory if it doesn't exist
                var directory = Path.GetDirectoryName(filePath);
                Debug.WriteLine($"Export directory: {directory}");
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                Debug.WriteLine("Creating Excel workbook");
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Transactions");

                Debug.WriteLine("Adding headers");
                // Add headers
                worksheet.Cell(1, 1).Value = "Date";
                worksheet.Cell(1, 2).Value = "Type";
                worksheet.Cell(1, 3).Value = "Category";
                worksheet.Cell(1, 4).Value = "Amount";
                worksheet.Cell(1, 5).Value = "Notes";

                Debug.WriteLine("Adding transaction data");
                // Add data
                int row = 2;
                foreach (var transaction in transactions)
                {
                    worksheet.Cell(row, 1).Value = transaction.Date;
                    worksheet.Cell(row, 2).Value = transaction.Type.ToString();
                    worksheet.Cell(row, 3).Value = transaction.Category;
                    worksheet.Cell(row, 4).Value = transaction.Amount;
                    worksheet.Cell(row, 5).Value = transaction.Notes;
                    row++;
                }

                Debug.WriteLine("Adjusting column widths");
                worksheet.Columns().AdjustToContents();

                Debug.WriteLine($"Saving workbook to: {filePath}");
                workbook.SaveAs(filePath);
                Debug.WriteLine("Excel export completed successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Excel export failed: {ex.GetType().Name} - {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        public async Task ExportToPdfAsync(string filePath, DateTime startDate, DateTime endDate)
        {
            try
            {
                var transactions = await GetTransactionsByDateRangeAsync(startDate, endDate);
                Debug.WriteLine($"Retrieved {transactions.Count} transactions for PDF export");

                // Create directory if it doesn't exist
                var directory = Path.GetDirectoryName(filePath);
                Debug.WriteLine($"PDF export directory: {directory}");
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                Debug.WriteLine("Creating PDF document");
                using var writer = new PdfWriter(filePath);
                using var pdf = new PdfDocument(writer);
                using var document = new Document(pdf);

                Debug.WriteLine("Adding title");
                // Add title
                document.Add(new Paragraph($"Transaction Report ({startDate:d} - {endDate:d})"));
                document.Add(new Paragraph("\n"));

                Debug.WriteLine("Creating table");
                // Create table
                var table = new Table(5);
                table.AddCell("Date");
                table.AddCell("Type");
                table.AddCell("Category");
                table.AddCell("Amount");
                table.AddCell("Notes");

                Debug.WriteLine("Adding transaction data to table");
                foreach (var transaction in transactions)
                {
                    table.AddCell(transaction.Date.ToString("d"));
                    table.AddCell(transaction.Type.ToString());
                    table.AddCell(transaction.Category);
                    table.AddCell(transaction.Amount.ToString("C"));
                    table.AddCell(transaction.Notes ?? "");
                }

                Debug.WriteLine("Adding table to document");
                document.Add(table);
                Debug.WriteLine("PDF export completed successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PDF export failed: {ex.GetType().Name} - {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }
    }
}