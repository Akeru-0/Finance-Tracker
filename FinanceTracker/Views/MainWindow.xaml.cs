using FinanceTracker.Data;
using FinanceTracker.Services;
using FinanceTracker.ViewModels;
using System.Windows;

namespace FinanceTracker.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Initialize database context and services
            var dbContext = new FinanceDbContext();
            var transactionService = new TransactionService(dbContext);

            // Set the DataContext to our MainViewModel
            DataContext = new MainViewModel(transactionService);
        }
    }
}