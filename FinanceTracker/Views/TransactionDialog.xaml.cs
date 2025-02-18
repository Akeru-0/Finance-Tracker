using FinanceTracker.Models;
using FinanceTracker.ViewModels;
using System.Windows;

namespace FinanceTracker.Views
{
    public partial class TransactionDialog : Window
    {
        public TransactionDialog(Transaction? transaction = null)
        {
            InitializeComponent();
            var viewModel = new TransactionDialogViewModel(transaction);
            DataContext = viewModel;

            // Close the dialog when the ViewModel sets the DialogResult
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(TransactionDialogViewModel.DialogResult) && 
                    viewModel.DialogResult.HasValue)
                {
                    DialogResult = viewModel.DialogResult;
                    Close();
                }
            };
        }

        public Transaction? Transaction => (DataContext as TransactionDialogViewModel)?.Transaction;
    }
}