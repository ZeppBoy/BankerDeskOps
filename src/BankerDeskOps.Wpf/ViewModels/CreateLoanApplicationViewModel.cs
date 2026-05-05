using BankerDeskOps.Application.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace BankerDeskOps.Wpf.ViewModels
{
    public partial class CreateLoanApplicationViewModel : ObservableObject, INotifyDataErrorInfo
    {
        private readonly Action<CreateLoanApplicationRequest> _onCreated;
        private readonly Action _onCancel;
        private Dictionary<string, List<string>> _errors = new();

        [ObservableProperty]
        private ObservableCollection<ProductDto> products = new();

        [ObservableProperty]
        private ProductDto? selectedProduct;

        [ObservableProperty]
        private decimal amount = 0m;

        [ObservableProperty]
        private int termMonths = 12;

        [ObservableProperty]
        private string? errorMessage;

        public bool HasErrors => _errors.Values.Any(e => e.Count > 0);
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public CreateLoanApplicationViewModel(
            ObservableCollection<ProductDto> products,
            Action<CreateLoanApplicationRequest> onCreated,
            Action onCancel)
        {
            Products = products;
            _onCreated = onCreated ?? throw new ArgumentNullException(nameof(onCreated));
            _onCancel = onCancel ?? throw new ArgumentNullException(nameof(onCancel));
        }

        [RelayCommand]
        public void Create()
        {
            var validationErrors = Validate();
            if (validationErrors.Count > 0)
            {
                ErrorMessage = string.Join("; ", validationErrors.SelectMany(e => e.Value));
                return;
            }

            var request = new CreateLoanApplicationRequest
            {
                ProductId = SelectedProduct!.Id,
                Amount = Amount,
                TermMonths = TermMonths
            };

            _onCreated(request);
        }

        [RelayCommand]
        public void Cancel()
        {
            _onCancel();
        }

        private Dictionary<string, List<string>> Validate()
        {
            var errors = new Dictionary<string, List<string>>();

            if (SelectedProduct == null)
                errors["Product"] = new List<string> { "Please select a product." };

            if (Amount <= 0)
                errors["Amount"] = new List<string> { "Amount must be greater than zero." };

            if (TermMonths <= 0)
                errors["TermMonths"] = new List<string> { "Term months must be greater than zero." };

            _errors = errors;
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(string.Empty));
            return errors;
        }

        public System.Collections.IEnumerable GetErrors(string? propertyName)
        {
            if (propertyName == null || !_errors.TryGetValue(propertyName, out var errors))
                return Enumerable.Empty<string>();
            return errors;
        }
    }
}
