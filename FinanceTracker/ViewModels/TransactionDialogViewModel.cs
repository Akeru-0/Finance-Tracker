using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinanceTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace FinanceTracker.ViewModels
{
    public partial class TransactionDialogViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _title;

        private Transaction? _transaction;
        public Transaction? Transaction
        {
            get => _transaction;
            set
            {
                if (_transaction != null)
                {
                    _transaction.PropertyChanged -= Transaction_PropertyChanged;
                }

                _transaction = value;
                
                if (_transaction != null)
                {
                    _transaction.PropertyChanged += Transaction_PropertyChanged;
                    UpdateCategories(_transaction.Type);
                }
                
                OnPropertyChanged(nameof(Transaction));
            }
        }

        private void Transaction_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Transaction.Type) && Transaction != null)
            {
                UpdateCategories(Transaction.Type);
            }
        }

        private static readonly string[] IncomeCategories = new[]
        {
            "Salary", "Freelance", "Investments", "Gifts", "Other Income"
        };

        private static readonly string[] ExpenseCategories = new[]
        {
            "Food & Dining", "Shopping", "Housing", "Transportation", "Entertainment",
            "Healthcare", "Insurance", "Utilities", "Education", "Travel",
            "Personal Care", "Gifts & Donations", "Other Expenses"
        };

        private IEnumerable<string> _categories = ExpenseCategories;
        public IEnumerable<string> Categories 
        { 
            get => _categories;
            private set
            {
                if (_categories != value)
                {
                    _categories = value;
                    OnPropertyChanged(nameof(Categories));
                }
            }
        }

        public IEnumerable<TransactionType> TransactionTypes => Enum.GetValues(typeof(TransactionType)).Cast<TransactionType>();
        public IEnumerable<RecurringPeriod> RecurringPeriods => Enum.GetValues(typeof(RecurringPeriod)).Cast<RecurringPeriod>();

        [ObservableProperty]
        private bool? _dialogResult;

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public TransactionDialogViewModel(Transaction? transaction = null)
        {
            Title = transaction == null ? "Add Transaction" : "Edit Transaction";
            
            // Initialize with default transaction if none provided
            Transaction = transaction ?? new Transaction
            {
                Date = DateTime.Now,
                Type = TransactionType.Expense
            };

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void UpdateCategories(TransactionType type)
        {
            Categories = type == TransactionType.Income ? IncomeCategories : ExpenseCategories;
            
            // Ensure the current category is valid for the new type
            if (Transaction != null && !Categories.Contains(Transaction.Category))
            {
                Transaction.Category = Categories.First();
            }
        }

        private void Save()
        {
            DialogResult = true;
        }

        private void Cancel()
        {
            DialogResult = false;
        }
    }
}