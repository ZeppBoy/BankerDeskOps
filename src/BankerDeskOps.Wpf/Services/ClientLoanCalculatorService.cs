using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace BankerDeskOps.Wpf.Services
{
    public class ClientLoanCalculatorService : ILoanCalculatorService
    {
        private readonly RateApiService _rateApiService;
        private readonly FeeApiService _feeApiService;
        private readonly CommissionApiService _commissionApiService;
        private readonly ILogger<ClientLoanCalculatorService> _logger;

        public ClientLoanCalculatorService(
            RateApiService rateApiService,
            FeeApiService feeApiService,
            CommissionApiService commissionApiService,
            ILogger<ClientLoanCalculatorService> logger)
        {
            _rateApiService = rateApiService ?? throw new ArgumentNullException(nameof(rateApiService));
            _feeApiService = feeApiService ?? throw new ArgumentNullException(nameof(feeApiService));
            _commissionApiService = commissionApiService ?? throw new ArgumentNullException(nameof(commissionApiService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<LoanCalculationResultDto> CalculateAsync(LoanCalculationRequest request)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));
            if (request.Amount <= 0) throw new ArgumentException("Amount must be greater than zero.");
            if (request.TermMonths <= 0) throw new ArgumentException("Term months must be greater than zero.");

            _logger.LogInformation("Calculating loan for product {ProductId}, amount {Amount}, term {Term} months",
                request.ProductId, request.Amount, request.TermMonths);

            var rates = await _rateApiService.GetRatesByProductIdAsync(request.ProductId);
            var applicableRate = rates.FirstOrDefault(r =>
                request.Amount >= r.MinAmount &&
                request.Amount <= r.MaxAmount &&
                request.TermMonths >= r.MinTermMonths &&
                request.TermMonths <= r.MaxTermMonths);

            if (applicableRate is null)
                throw new InvalidOperationException("No applicable rate found for the given parameters.");

            var fees = await _feeApiService.GetFeesByProductIdAsync(request.ProductId);
            var commissions = await _commissionApiService.GetCommissionsByProductIdAsync(request.ProductId);

            decimal totalFees = fees.Sum(f => f.Amount);
            decimal totalCommissionPercentage = commissions.Sum(c => c.Percentage);

            decimal annualRate = applicableRate.RateValue / 100m;
            decimal monthlyRate = annualRate / 12m;

            decimal monthlyPayment;
            if (monthlyRate == 0)
                monthlyPayment = request.Amount / request.TermMonths;
            else
            {
                var factor = Math.Pow(1 + (double)monthlyRate, request.TermMonths);
                monthlyPayment = (decimal)((double)request.Amount * (double)monthlyRate * factor / (factor - 1));
            }

            decimal totalAmount = monthlyPayment * request.TermMonths;
            decimal totalInterest = totalAmount - request.Amount;

            var schedule = GenerateSchedule(request.Amount, monthlyPayment, monthlyRate, request.TermMonths);

            _logger.LogInformation("Calculation complete. Monthly payment: {MonthlyPayment}, Total interest: {TotalInterest}",
                Math.Round(monthlyPayment, 2), Math.Round(totalInterest, 2));

            return new LoanCalculationResultDto
            {
                MonthlyPayment = Math.Round(monthlyPayment, 2),
                TotalAmount = Math.Round(totalAmount + totalFees, 2),
                TotalInterest = Math.Round(totalInterest, 2),
                InterestRate = applicableRate.RateValue,
                Schedule = schedule
            };
        }

        private static List<RepaymentScheduleDto> GenerateSchedule(
            decimal principal,
            decimal monthlyPayment,
            decimal monthlyRate,
            int termMonths)
        {
            var schedule = new List<RepaymentScheduleDto>();
            decimal remainingBalance = principal;
            DateTime startDate = DateTime.UtcNow;

            for (int i = 1; i <= termMonths; i++)
            {
                decimal interestPayment = remainingBalance * monthlyRate;
                decimal principalPayment = monthlyPayment - interestPayment;

                if (i == termMonths)
                {
                    principalPayment = remainingBalance;
                    monthlyPayment = principalPayment + interestPayment;
                }

                schedule.Add(new RepaymentScheduleDto
                {
                    ScheduleId = Guid.NewGuid(),
                    PaymentNumber = i,
                    DueDate = startDate.AddMonths(i),
                    PrincipalAmount = Math.Round(principalPayment, 2),
                    InterestAmount = Math.Round(interestPayment, 2),
                    TotalPayment = Math.Round(monthlyPayment, 2)
                });

                remainingBalance -= principalPayment;
            }

            return schedule;
        }
    }
}
