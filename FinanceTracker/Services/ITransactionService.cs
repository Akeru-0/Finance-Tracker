using FinanceTracker.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinanceTracker.Services
{
    public interface ITransactionService
    {
        Task<List<Transaction>> GetAllTransactionsAsync();
        Task<List<Transaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<Transaction>> GetTransactionsByCategoryAsync(string category);
        Task<List<Transaction>> GetTransactionsByTypeAsync(TransactionType type);
        Task<Transaction?> GetTransactionByIdAsync(int id);
        Task<bool> AddTransactionAsync(Transaction transaction);
        Task<bool> UpdateTransactionAsync(Transaction transaction);
        Task<bool> DeleteTransactionAsync(int id);
        Task<decimal> GetBalanceAsync();
        Task<Dictionary<string, decimal>> GetCategoryTotalsAsync(TransactionType type, DateTime startDate, DateTime endDate);
        Task<List<KeyValuePair<DateTime, decimal>>> GetDailyTotalsAsync(DateTime startDate, DateTime endDate, TransactionType type);
        Task ExportToExcelAsync(string filePath, DateTime startDate, DateTime endDate);
        Task ExportToPdfAsync(string filePath, DateTime startDate, DateTime endDate);
    }
}