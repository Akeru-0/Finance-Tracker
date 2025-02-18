using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FinanceTracker.ViewModels
{
    public class ViewModelBase : ObservableObject
    {
        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        protected void SetBusy(string message)
        {
            IsBusy = true;
            StatusMessage = message;
        }

        protected void ClearBusy()
        {
            IsBusy = false;
            StatusMessage = string.Empty;
        }
    }
}