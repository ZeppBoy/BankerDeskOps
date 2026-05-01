using System;
using BankerDeskOps.Domain.Entities;
using BankerDeskOps.Domain.Enums;

namespace BankerDeskOps.Application.Services.Financial
{
    public interface IFinancialCalculator
    {
        decimal CalculateMonthlyPayment(decimal principal, decimal annualRate, int termMonths);
        decimal CalculateTotalInterest(decimal principal, decimal annualRate, int termMonths);
        RepaymentSchedule[] GenerateRepaymentSchedule(decimal principal, decimal annualRate, int termMonths, DateTime startDate);
        decimal CalculateEIR(RepaymentSchedule[] schedules, decimal loanAmount);
        decimal CalculateDailyInterest(decimal principal, decimal annualRate);
    }

    public class FinancialCalculator : IFinancialCalculator
    {
        public decimal CalculateMonthlyPayment(decimal principal, decimal annualRate, int termMonths)
        {
            if (principal <= 0) throw new ArgumentException("Principal must be positive.");
            if (termMonths <= 0) throw new ArgumentException("Term must be positive.");

            if (annualRate == 0)
                return Math.Round(principal / termMonths, 2);

            decimal monthlyRate = annualRate / 12m;
            decimal factor = Power(1 + monthlyRate, termMonths);
            decimal payment = principal * monthlyRate * factor / (factor - 1);

            return Math.Round(payment, 2);
        }

        public decimal CalculateTotalInterest(decimal principal, decimal annualRate, int termMonths)
        {
            decimal monthlyPayment = CalculateMonthlyPayment(principal, annualRate, termMonths);
            decimal totalPaid = monthlyPayment * termMonths;
            return Math.Round(totalPaid - principal, 2);
        }

        public RepaymentSchedule[] GenerateRepaymentSchedule(decimal principal, decimal annualRate, int termMonths, DateTime startDate)
        {
            var schedules = new RepaymentSchedule[termMonths];
            decimal balance = principal;
            decimal monthlyPayment = CalculateMonthlyPayment(principal, annualRate, termMonths);
            decimal monthlyRate = annualRate / 12m;

            DateTime currentDate = startDate;

            for (int i = 0; i < termMonths; i++)
            {
                decimal interestPayment = Math.Round(balance * monthlyRate, 2);
                decimal capitalPayment = Math.Round(monthlyPayment - interestPayment, 2);

                if (i == termMonths - 1)
                {
                    capitalPayment = balance;
                    monthlyPayment = Math.Round(capitalPayment + interestPayment, 2);
                }

                balance = Math.Round(balance - capitalPayment, 2);
                currentDate = currentDate.AddMonths(1);

                schedules[i] = new RepaymentSchedule
                {
                    ScheduleId = Guid.NewGuid(),
                    LoanApplicationId = Guid.Empty,
                    PlannedDate = currentDate,
                    Capital = capitalPayment,
                    Interest = interestPayment,
                    Saldo = Math.Max(balance, 0),
                    EIR = annualRate
                };
            }

            return schedules;
        }

        public decimal CalculateEIR(RepaymentSchedule[] schedules, decimal loanAmount)
        {
            if (schedules == null || schedules.Length == 0)
                throw new ArgumentException("No repayment schedules provided.");

            double rate = EstimateInitialRate(schedules, loanAmount);
            double tolerance = 1e-10;
            int maxIterations = 200;

            for (int i = 0; i < maxIterations; i++)
            {
                double npv = CalculateNPV(schedules, loanAmount, rate);
                double derivative = CalculateDerivative(schedules, loanAmount, rate);

                if (Math.Abs(derivative) < 1e-15)
                    break;

                double newRate = rate - npv / derivative;

                if (Math.Abs(newRate - rate) < tolerance)
                {
                    rate = newRate;
                    break;
                }

                rate = newRate;
            }

            return Math.Round((decimal)(rate * 100), 4);
        }

        public decimal CalculateDailyInterest(decimal principal, decimal annualRate)
        {
            return Math.Round(principal * annualRate / 365m, 2);
        }

        private static double CalculateNPV(RepaymentSchedule[] schedules, decimal loanAmount, double rate)
        {
            double npv = -(double)loanAmount;

            for (int i = 0; i < schedules.Length; i++)
            {
                int days = (schedules[i].PlannedDate - schedules[0].PlannedDate.AddMonths(-1)).Days;
                double discountFactor = Math.Pow(1 + rate, days / 365.0);
                npv -= (double)(schedules[i].Capital + schedules[i].Interest) / discountFactor;
            }

            return npv;
        }

        private static double CalculateDerivative(RepaymentSchedule[] schedules, decimal loanAmount, double rate)
        {
            double derivative = 0;

            for (int i = 0; i < schedules.Length; i++)
            {
                int days = (schedules[i].PlannedDate - schedules[0].PlannedDate.AddMonths(-1)).Days;
                double discountFactor = Math.Pow(1 + rate, days / 365.0);
                derivative -= (days / 365.0) * (double)(schedules[i].Capital + schedules[i].Interest) / ((discountFactor) * (1 + rate));
            }

            return derivative;
        }

        private static double EstimateInitialRate(RepaymentSchedule[] schedules, decimal loanAmount)
        {
            decimal totalPayments = 0;
            foreach (var schedule in schedules)
                totalPayments += schedule.Capital + schedule.Interest;

            int years = Math.Max(schedules.Length / 12, 1);
            double rate = Math.Pow((double)(totalPayments / loanAmount), 1.0 / years) - 1;

            return Math.Max(rate, 0.001);
        }

        private static decimal Power(decimal baseValue, int exponent)
        {
            return (decimal)Math.Pow((double)baseValue, exponent);
        }
    }
}
