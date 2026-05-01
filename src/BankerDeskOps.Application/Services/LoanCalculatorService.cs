using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Application.Interfaces;

namespace BankerDeskOps.Application.Services
{
    public class LoanCalculatorService : ILoanCalculatorService
    {
        private readonly IRateRepository _rateRepository;
        private readonly IFeeRepository _feeRepository;
        private readonly ICommissionRepository _commissionRepository;

        public LoanCalculatorService(
            IRateRepository rateRepository,
            IFeeRepository feeRepository,
            ICommissionRepository commissionRepository)
        {
            _rateRepository = rateRepository ?? throw new ArgumentNullException(nameof(rateRepository));
            _feeRepository = feeRepository ?? throw new ArgumentNullException(nameof(feeRepository));
            _commissionRepository = commissionRepository ?? throw new ArgumentNullException(nameof(commissionRepository));
        }

        public async Task<LoanCalculationResultDto> CalculateAsync(LoanCalculationRequest request)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));
            if (request.Amount <= 0) throw new ArgumentException("Amount must be greater than zero.");
            if (request.TermMonths <= 0) throw new ArgumentException("Term months must be greater than zero.");

            var rates = await _rateRepository.GetByProductIdAsync(request.ProductId);
            var applicableRate = rates.FirstOrDefault(r =>
                request.Amount >= r.MinAmount &&
                request.Amount <= r.MaxAmount &&
                request.TermMonths >= r.MinTermMonths &&
                request.TermMonths <= r.MaxTermMonths);

            if (applicableRate is null)
                throw new InvalidOperationException("No applicable rate found for the given parameters.");

            var fees = await _feeRepository.GetByProductIdAsync(request.ProductId);
            var commissions = await _commissionRepository.GetByProductIdAsync(request.ProductId);

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
