using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace BankerDeskOps.Application.Services
{
    public class CreateRepaymentScheduleFromApplicationService
    {
        private readonly IRepaymentScheduleRepository _repaymentScheduleRepository;
        private readonly ILogger<CreateRepaymentScheduleFromApplicationService> _logger;

        public CreateRepaymentScheduleFromApplicationService(
            IRepaymentScheduleRepository repaymentScheduleRepository,
            ILogger<CreateRepaymentScheduleFromApplicationService> logger)
        {
            _repaymentScheduleRepository = repaymentScheduleRepository ?? throw new ArgumentNullException(nameof(repaymentScheduleRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<RepaymentScheduleDto>> GenerateAndSaveScheduleAsync(LoanApplicationDto loanApplication, DateTime firstPaymentDate)
        {
            if (loanApplication == null) throw new ArgumentNullException(nameof(loanApplication));
            
            try
            {
                _logger.LogInformation("Generating repayment schedule for application {AppId}", loanApplication.Id);

                var schedules = new List<RepaymentScheduleDto>();
                
                var calculator = new Financial.FinancialCalculator();
                
                decimal annualRate = await GetAnnualInterestRateAsync(loanApplication.ProductId);
                if (annualRate == 0)
                    throw new InvalidOperationException("Unable to retrieve interest rate for this product.");

                int termMonths = loanApplication.TermMonths;
                decimal monthlyRate = annualRate / 12m;
                
                decimal monthlyPayment = calculator.CalculateMonthlyPayment(loanApplication.Amount, annualRate, termMonths);
                decimal remainingBalance = loanApplication.Amount;
                DateTime paymentDate = firstPaymentDate;

                for (int i = 1; i <= termMonths; i++)
                {
                    decimal interestAmount = Math.Round(remainingBalance * monthlyRate, 2);
                    decimal principalAmount = Math.Round(monthlyPayment - interestAmount, 2);

                    if (i == termMonths)
                    {
                        principalAmount = remainingBalance;
                        monthlyPayment = Math.Round(principalAmount + interestAmount, 2);
                    }

                    var scheduleEntry = new RepaymentScheduleDto
                    {
                        ScheduleId = Guid.NewGuid(),
                        LoanApplicationId = loanApplication.Id,
                        PaymentNumber = i,
                        DueDate = paymentDate,
                        PrincipalAmount = principalAmount,
                        InterestAmount = interestAmount,
                        TotalPayment = Math.Round(monthlyPayment, 2)
                    };

                    schedules.Add(scheduleEntry);

                    remainingBalance -= principalAmount;
                    paymentDate = paymentDate.AddMonths(1);
                }

                _logger.LogInformation("Generated {Count} schedule entries for application {AppId}", schedules.Count, loanApplication.Id);
                
                return schedules;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate repayment schedule for application {AppId}: {Message}", 
                    loanApplication.Id, ex.Message);
                throw;
            }
        }

        private async Task<decimal> GetAnnualInterestRateAsync(Guid productId)
        {
            try
            {
                var calculator = new Financial.FinancialCalculator();
                
                var request = new LoanCalculationRequest
                {
                    ProductId = productId,
                    Amount = 10000m,
                    TermMonths = 12
                };

                var loanCalculatorServiceType = Type.GetType("BankerDeskOps.Application.Services.LoanCalculatorService, BankerDeskOps.Application");
                if (loanCalculatorServiceType == null)
                    throw new InvalidOperationException("Cannot find LoanCalculatorService type.");

                var instance = Activator.CreateInstance(loanCalculatorServiceType);
                if (instance == null)
                    throw new InvalidOperationException("Cannot create LoanCalculatorService instance.");

                var calculateMethod = loanCalculatorServiceType.GetMethod("CalculateAsync");
                if (calculateMethod == null)
                    throw new InvalidOperationException("Cannot find CalculateAsync method.");

                var result = await (Task<LoanCalculationResultDto>)calculateMethod.Invoke(instance, [request])!;
                
                return result.InterestRate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get annual interest rate for product {ProductId}: {Message}", 
                    productId, ex.Message);
                return 0m;
            }
        }
    }
}
