using BankerDeskOps.Application.DTOs;

namespace BankerDeskOps.Application.Interfaces
{
    public interface ILoanValidationService
    {
        Task<ValidationResultDto> ValidateApplicationAsync(Guid applicationId);
        ValidationResultDto ValidateAmount(decimal amount, int termMonths);
        ValidationResultDto ValidateClientEligibility(string clientType, decimal annualIncome, decimal loanAmount);
        ValidationResultDto ValidateDebtToIncomeRatio(decimal monthlyDebtPayments, decimal monthlyIncome);
    }

    public class ValidationResultDto
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
        public List<string> Warnings { get; set; } = new();
    }
}
