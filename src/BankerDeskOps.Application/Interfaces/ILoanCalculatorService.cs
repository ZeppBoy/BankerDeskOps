using BankerDeskOps.Application.DTOs;

namespace BankerDeskOps.Application.Interfaces
{
    public interface ILoanCalculatorService
    {
        Task<LoanCalculationResultDto> CalculateAsync(LoanCalculationRequest request);
    }
}
