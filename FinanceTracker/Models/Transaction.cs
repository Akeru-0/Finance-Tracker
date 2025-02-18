using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.ComponentModel.DataAnnotations;

namespace FinanceTracker.Models
{
    public partial class Transaction : ObservableValidator
    {
        [Key]
        public int Id { get; set; }

        [ObservableProperty]
        [Required]
        private decimal _amount;

        [ObservableProperty]
        [Required]
        private DateTime _date = DateTime.Now;

        [ObservableProperty]
        [Required]
        private string _category = string.Empty;

        [ObservableProperty]
        [Required]
        private TransactionType _type;

        [ObservableProperty]
        private string? _notes;

        [ObservableProperty]
        private bool _isRecurring;

        [ObservableProperty]
        private RecurringPeriod? _recurringPeriod;
    }

    public enum TransactionType
    {
        Income,
        Expense
    }

    public enum RecurringPeriod
    {
        Daily,
        Weekly,
        Monthly,
        Yearly
    }
}