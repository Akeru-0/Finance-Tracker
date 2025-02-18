using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinanceTracker.Models;
using FinanceTracker.Services;
using FinanceTracker.Views;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.Measure;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FinanceTracker.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        private readonly ITransactionService _transactionService;
        private readonly Dictionary<string, SKColor> _categoryColors = new()
        {
            // Income categories
            {"Salary", new SKColor(76, 175, 80)},        // Green
            {"Freelance", new SKColor(139, 195, 74)},    // Light Green
            {"Investments", new SKColor(205, 220, 57)},   // Lime
            {"Gifts", new SKColor(255, 235, 59)},        // Yellow
            {"Other Income", new SKColor(255, 193, 7)},   // Amber

            // Expense categories
            {"Food & Dining", new SKColor(244, 67, 54)},     // Red
            {"Shopping", new SKColor(233, 30, 99)},          // Pink
            {"Housing", new SKColor(156, 39, 176)},          // Purple
            {"Transportation", new SKColor(103, 58, 183)},    // Deep Purple
            {"Entertainment", new SKColor(33, 150, 243)},     // Blue
            {"Healthcare", new SKColor(0, 188, 212)},        // Cyan
            {"Insurance", new SKColor(0, 150, 136)},         // Teal
            {"Utilities", new SKColor(96, 125, 139)},        // Blue Grey
            {"Education", new SKColor(255, 152, 0)},         // Orange
            {"Travel", new SKColor(255, 87, 34)},           // Deep Orange
            {"Personal Care", new SKColor(121, 85, 72)},     // Brown
            {"Gifts & Donations", new SKColor(158, 158, 158)}, // Grey
            {"Other Expenses", new SKColor(66, 66, 66)}      // Dark Grey
        };

        public MainViewModel(ITransactionService transactionService)
        {
            _transactionService = transactionService;
            _transactions = new ObservableCollection<Transaction>();
            Transactions = _transactions;
            InitializeCommands();
            LoadDataAsync();

            // Subscribe to property changes of individual transactions
            _transactions.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (Transaction item in e.NewItems)
                    {
                        item.PropertyChanged += Transaction_PropertyChanged;
                    }
                }
                if (e.OldItems != null)
                {
                    foreach (Transaction item in e.OldItems)
                    {
                        item.PropertyChanged -= Transaction_PropertyChanged;
                    }
                }
            };
        }

        private async void Transaction_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            await UpdateChartsAsync();
        }

        private ObservableCollection<Transaction> _transactions;
        public ObservableCollection<Transaction> Transactions
        {
            get => _transactions;
            private set
            {
                if (_transactions != value)
                {
                    if (_transactions != null)
                    {
                        _transactions.CollectionChanged -= Transactions_CollectionChanged;
                    }
                    _transactions = value;
                    if (_transactions != null)
                    {
                        _transactions.CollectionChanged += Transactions_CollectionChanged;
                    }
                    OnPropertyChanged(nameof(Transactions));
                }
            }
        }

        private async void Transactions_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            await UpdateChartsAsync();
        }

        [ObservableProperty]
        private Transaction? _selectedTransaction;

        [ObservableProperty]
        private decimal _currentBalance;

        [ObservableProperty]
        private DateTime _startDate = DateTime.Now.AddDays(-31);

        [ObservableProperty]
        private DateTime _endDate = DateTime.Now;

        partial void OnStartDateChanged(DateTime value)
        {
            _ = UpdateChartsAsync();
        }

        partial void OnEndDateChanged(DateTime value)
        {
            _ = UpdateChartsAsync();
        }

        [ObservableProperty]
        private ISeries[] _expensesByCategorySeries = Array.Empty<ISeries>();

        [ObservableProperty]
        private ISeries[] _monthlyTrendSeries = Array.Empty<ISeries>();

        [ObservableProperty]
        private Axis[] _xAxes = new[]
        {
            new Axis
            {
                Labeler = value => {
                    var date = new DateTime((long)value);
                    return date.Day == 1 ? date.ToString("MMM d") : date.Day.ToString();
                },
                LabelsRotation = 0,
                MinStep = TimeSpan.FromDays(1).Ticks
            }
        };

        [ObservableProperty]
        private Axis[] _yAxes = new[]
        {
            new Axis
            {
                Labeler = value => value.ToString("C0")
            }
        };

        public ICommand AddTransactionCommand { get; private set; }
        public ICommand EditTransactionCommand { get; private set; }
        public ICommand DeleteTransactionCommand { get; private set; }
        public ICommand RefreshCommand { get; private set; }

        private void InitializeCommands()
        {
            AddTransactionCommand = new AsyncRelayCommand(AddTransactionAsync);
            EditTransactionCommand = new AsyncRelayCommand(EditTransactionAsync, CanEditTransaction);
            DeleteTransactionCommand = new AsyncRelayCommand(DeleteTransactionAsync, CanDeleteTransaction);
            RefreshCommand = new AsyncRelayCommand(LoadDataAsync);
        }

        private async Task LoadDataAsync()
        {
            try
            {
                SetBusy("Loading data...");
                
                var transactions = await _transactionService.GetAllTransactionsAsync();
                
                // Unsubscribe from old transactions
                foreach (var transaction in Transactions)
                {
                    transaction.PropertyChanged -= Transaction_PropertyChanged;
                }
                
                Transactions.Clear();
                
                // Add new transactions and subscribe to their property changes
                foreach (var transaction in transactions)
                {
                    transaction.PropertyChanged += Transaction_PropertyChanged;
                    Transactions.Add(transaction);
                }

                CurrentBalance = await _transactionService.GetBalanceAsync();
                await UpdateChartsAsync();
                
                ClearBusy();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading data: {ex.Message}";
            }
        }

        private async Task UpdateChartsAsync()
        {
            try
            {
                // Update expenses by category chart
                var categoryTotals = await _transactionService.GetCategoryTotalsAsync(
                    TransactionType.Expense, StartDate.Date, EndDate.Date.AddDays(1).AddSeconds(-1));

                ExpensesByCategorySeries = categoryTotals
                    .OrderByDescending(kvp => kvp.Value)
                    .Select(kvp =>
                        new PieSeries<decimal>
                        {
                            Values = new[] { kvp.Value },
                            Name = $"{kvp.Key} ({kvp.Value:C})",
                            Fill = new SolidColorPaint(_categoryColors.TryGetValue(kvp.Key, out var color)
                                ? color
                                : new SKColor(128, 128, 128)), // Default grey for unknown categories
                            TooltipLabelFormatter = (point) => $"{kvp.Key}: {point.PrimaryValue:C}"
                        }).ToArray();

                // Update monthly trend chart
                // Get initial balance up to start date
                var initialTransactions = await _transactionService.GetTransactionsByDateRangeAsync(
                    DateTime.MinValue, StartDate.AddDays(-1));
                decimal initialBalance = initialTransactions.Sum(t =>
                    t.Type == TransactionType.Income ? t.Amount : -t.Amount);

                // Get transactions for the selected period
                var transactions = await _transactionService.GetTransactionsByDateRangeAsync(StartDate, EndDate);
                var sortedTransactions = transactions.OrderBy(t => t.Date).ToList();

                var balancePoints = new List<KeyValuePair<DateTime, decimal>>();
                decimal runningBalance = initialBalance;

                // Add initial balance point at start date
                balancePoints.Add(new KeyValuePair<DateTime, decimal>(StartDate, runningBalance));

                foreach (var transaction in sortedTransactions)
                {
                    runningBalance += transaction.Type == TransactionType.Income ?
                        transaction.Amount : -transaction.Amount;
                    balancePoints.Add(new KeyValuePair<DateTime, decimal>(
                        transaction.Date, runningBalance));
                }

                // Add final balance point at end date if no transactions on that day
                if (!balancePoints.Any(p => p.Key.Date == EndDate.Date))
                {
                    balancePoints.Add(new KeyValuePair<DateTime, decimal>(EndDate, runningBalance));
                }

                MonthlyTrendSeries = new ISeries[]
                {
                    new LineSeries<KeyValuePair<DateTime, decimal>>
                    {
                        Values = balancePoints,
                        Name = "Balance",
                        Stroke = new SolidColorPaint(new SKColor(33, 150, 243)) { StrokeThickness = 4 }, // Light blue
                        Fill = new SolidColorPaint(new SKColor(33, 150, 243, 40)), // Light blue with 40 alpha (transparency)
                        GeometryStroke = new SolidColorPaint(new SKColor(33, 150, 243)),
                        GeometryFill = new SolidColorPaint(SKColors.White),
                        GeometrySize = 12,
                        Mapping = (value, point) =>
                        {
                            point.PrimaryValue = (float)value.Value;
                            point.SecondaryValue = value.Key.Ticks;
                        },
                        TooltipLabelFormatter = (point) =>
                            $"{new DateTime((long)point.SecondaryValue):MMM dd, yyyy}: {point.PrimaryValue:C}"
                    }
                };

                // Force refresh of expense categories
                var currentSeries = ExpensesByCategorySeries;
                ExpensesByCategorySeries = Array.Empty<ISeries>();
                ExpensesByCategorySeries = currentSeries;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error updating charts: {ex.Message}";
            }
        }

        private async Task AddTransactionAsync()
        {
            var dialog = new TransactionDialog();
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true && dialog.Transaction != null)
            {
                if (await _transactionService.AddTransactionAsync(dialog.Transaction))
                {
                    Transactions.Add(dialog.Transaction);
                    CurrentBalance = await _transactionService.GetBalanceAsync();
                    await UpdateChartsAsync();
                }
            }
        }

        private async Task EditTransactionAsync()
        {
            if (SelectedTransaction == null) return;

            var dialog = new TransactionDialog(SelectedTransaction);
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true && dialog.Transaction != null)
            {
                if (await _transactionService.UpdateTransactionAsync(dialog.Transaction))
                {
                    var index = Transactions.IndexOf(SelectedTransaction);
                    if (index != -1)
                    {
                        Transactions[index] = dialog.Transaction;
                    }
                    CurrentBalance = await _transactionService.GetBalanceAsync();
                    await UpdateChartsAsync();
                }
            }
        }

        private bool CanEditTransaction()
        {
            return SelectedTransaction != null;
        }

        private async Task DeleteTransactionAsync()
        {
            if (SelectedTransaction == null) return;

            var result = MessageBox.Show(
                "Are you sure you want to delete this transaction?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                if (await _transactionService.DeleteTransactionAsync(SelectedTransaction.Id))
                {
                    Transactions.Remove(SelectedTransaction);
                    CurrentBalance = await _transactionService.GetBalanceAsync();
                    await UpdateChartsAsync();
                }
            }
        }

        private bool CanDeleteTransaction()
        {
            return SelectedTransaction != null;
        }
    }
}