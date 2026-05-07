using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Application.Interfaces;
using BankerDeskOps.Application.Services.Financial;
using BankerDeskOps.Wpf.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace BankerDeskOps.Wpf.ViewModels
{
    public partial class CreateRepaymentScheduleViewModel : ObservableObject, INotifyDataErrorInfo
    {
        private readonly RepaymentScheduleApiService _scheduleApiService;
        private readonly ILoanCalculatorService _loanCalculatorService;
        private readonly ILogger<CreateRepaymentScheduleViewModel> _logger;
        private Dictionary<string, List<string>> _errors = new();

        public LoanApplicationDto LoanApplication { get; }

        [ObservableProperty]
        private decimal monthlyPayment;

        [ObservableProperty]
        private int paymentCount;

        [ObservableProperty]
        private DateTime firstPaymentDate;

        [ObservableProperty]
        private ObservableCollection<RepaymentScheduleDto> generatedSchedule = new();

        [ObservableProperty]
        private string? errorMessage;

        [ObservableProperty]
        private bool isLoading;

        public bool HasErrors => _errors.Values.Any(e => e.Count > 0);
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public CreateRepaymentScheduleViewModel(
            LoanApplicationDto loanApplication,
            RepaymentScheduleApiService scheduleApiService,
            ILoanCalculatorService loanCalculatorService,
            ILogger<CreateRepaymentScheduleViewModel> logger)
        {
            LoanApplication = loanApplication ?? throw new ArgumentNullException(nameof(loanApplication));
            _scheduleApiService = scheduleApiService ?? throw new ArgumentNullException(nameof(scheduleApiService));
            _loanCalculatorService = loanCalculatorService ?? throw new ArgumentNullException(nameof(loanCalculatorService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            FirstPaymentDate = DateTime.Today.AddMonths(1);
            PaymentCount = LoanApplication.TermMonths;
            CalculateMonthlyPayment();
        }

        private void CalculateMonthlyPayment()
        {
            try
            {
                var request = new LoanCalculationRequest
                {
                    ProductId = LoanApplication.ProductId,
                    Amount = LoanApplication.Amount,
                    TermMonths = LoanApplication.TermMonths
                };

                var result = _loanCalculatorService.CalculateAsync(request).GetAwaiter().GetResult();

                MonthlyPayment = Math.Round(result.MonthlyPayment, 2);
                PaymentCount = LoanApplication.TermMonths;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to calculate monthly payment: {Message}", ex.Message);
                ErrorMessage = $"Error calculating payment: {ex.Message}";
                MonthlyPayment = 0m;
            }
        }

        [RelayCommand]
        private void GenerateSchedule()
        {
            try
            {
                ErrorMessage = null;

                if (FirstPaymentDate < DateTime.Today.AddMonths(-1))
                {
                    ErrorMessage = "First payment date cannot be in the past.";
                    return;
                }

                GeneratedSchedule.Clear();

                decimal monthlyRate = LoanApplication.ProductId != Guid.Empty ? 
                    GetAnnualInterestRate() / 12m : 0m;

                if (monthlyRate == 0)
                {
                    ErrorMessage = "Unable to retrieve interest rate for this product.";
                    return;
                }

                decimal remainingBalance = LoanApplication.Amount;
                DateTime paymentDate = FirstPaymentDate;

                for (int i = 1; i <= PaymentCount; i++)
                {
                    decimal interestAmount = Math.Round(remainingBalance * monthlyRate, 2);
                    decimal principalAmount = Math.Round(MonthlyPayment - interestAmount, 2);

                    if (i == PaymentCount)
                    {
                        principalAmount = remainingBalance;
                        MonthlyPayment = Math.Round(principalAmount + interestAmount, 2);
                    }

                    var scheduleEntry = new RepaymentScheduleDto
                    {
                        ScheduleId = Guid.NewGuid(),
                        LoanApplicationId = LoanApplication.Id,
                        PaymentNumber = i,
                        DueDate = paymentDate,
                        PrincipalAmount = principalAmount,
                        InterestAmount = interestAmount,
                        TotalPayment = Math.Round(MonthlyPayment, 2)
                    };

                    GeneratedSchedule.Add(scheduleEntry);

                    remainingBalance -= principalAmount;
                    paymentDate = paymentDate.AddMonths(1);
                }

                _logger.LogInformation("Generated repayment schedule for application {AppId} with {Count} payments", 
                    LoanApplication.Id, PaymentCount);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error generating schedule: {ex.Message}";
                _logger.LogError("Failed to generate repayment schedule: {Message}", ex.Message);
            }
        }

        [RelayCommand]
        private async Task CreateScheduleEntries()
        {
            if (GeneratedSchedule.Count == 0)
            {
                ErrorMessage = "Please generate the schedule first.";
                return;
            }

            try
            {
                IsLoading = true;
                ErrorMessage = null;

                foreach (var entry in GeneratedSchedule)
                {
                    var request = new CreateRepaymentScheduleRequest
                    {
                        LoanApplicationId = entry.LoanApplicationId,
                        PaymentNumber = entry.PaymentNumber,
                        DueDate = entry.DueDate,
                        PrincipalAmount = entry.PrincipalAmount,
                        InterestAmount = entry.InterestAmount,
                        TotalPayment = entry.TotalPayment
                    };

                    await _scheduleApiService.CreateScheduleAsync(request);
                }

                _logger.LogInformation("Created {Count} schedule entries for application {AppId}", 
                    GeneratedSchedule.Count, LoanApplication.Id);
                
                ErrorMessage = $"Schedule with {GeneratedSchedule.Count} entries created successfully.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error creating schedule entries: {ex.Message}";
                _logger.LogError("Failed to create schedule entries: {Message}", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            ErrorMessage = null;
        }

        private decimal GetAnnualInterestRate()
        {
            try
            {
                var request = new LoanCalculationRequest
                {
                    ProductId = LoanApplication.ProductId,
                    Amount = LoanApplication.Amount,
                    TermMonths = LoanApplication.TermMonths
                };

                var result = _loanCalculatorService.CalculateAsync(request).GetAwaiter().GetResult();
                return result.InterestRate;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get interest rate: {Message}", ex.Message);
                return 0m;
            }
        }

        private Dictionary<string, List<string>> ValidateSchedule()
        {
            var errors = new Dictionary<string, List<string>>();

            if (PaymentCount <= 0)
                errors["PaymentCount"] = new List<string> { "Payment count must be greater than 0." };

            if (FirstPaymentDate < DateTime.Today.AddMonths(-1))
                errors["FirstPaymentDate"] = new List<string> { "First payment date cannot be in the past." };

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
