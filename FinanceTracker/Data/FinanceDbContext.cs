using FinanceTracker.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace FinanceTracker.Data
{
    public class FinanceDbContext : DbContext
    {
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string dbPath = Path.Combine(Directory.GetCurrentDirectory(), "FinanceTracker.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Transaction>()
                .Property(t => t.Amount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Category)
                .HasMaxLength(50);

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Notes)
                .HasMaxLength(500);

            // Seed sample transactions
            modelBuilder.Entity<Transaction>().HasData(
                new Transaction
                {
                    Id = 1,
                    Amount = 5000,
                    Date = System.DateTime.Now.AddDays(-20),
                    Category = "Salary",
                    Type = TransactionType.Income,
                    Notes = "Monthly salary",
                    IsRecurring = true,
                    RecurringPeriod = RecurringPeriod.Monthly
                },
                new Transaction
                {
                    Id = 2,
                    Amount = 1200,
                    Date = System.DateTime.Now.AddDays(-15),
                    Category = "Housing",
                    Type = TransactionType.Expense,
                    Notes = "Monthly rent",
                    IsRecurring = true,
                    RecurringPeriod = RecurringPeriod.Monthly
                },
                new Transaction
                {
                    Id = 3,
                    Amount = 300,
                    Date = System.DateTime.Now.AddDays(-10),
                    Category = "Food & Dining",
                    Type = TransactionType.Expense,
                    Notes = "Grocery shopping"
                },
                new Transaction
                {
                    Id = 4,
                    Amount = 150,
                    Date = System.DateTime.Now.AddDays(-5),
                    Category = "Transportation",
                    Type = TransactionType.Expense,
                    Notes = "Fuel"
                },
                new Transaction
                {
                    Id = 5,
                    Amount = 800,
                    Date = System.DateTime.Now.AddDays(-2),
                    Category = "Freelance",
                    Type = TransactionType.Income,
                    Notes = "Web development project"
                }
            );
        }
    }
}