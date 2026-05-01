using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Application.Interfaces;
using BankerDeskOps.Domain.Entities;

namespace BankerDeskOps.Application.Services
{
    public class LoanValidationService : ILoanValidationService
    {
        private readonly ILoanApplicationRepository _applicationRepository;
        private readonly IProductRepository _productRepository;

        public LoanValidationService(
            ILoanApplicationRepository applicationRepository,
            IProductRepository productRepository)
        {
            _applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        }

        public async Task<ValidationResultDto> ValidateApplicationAsync(Guid applicationId)
        {
            var result = new ValidationResultDto { IsValid = true };

            var application = await _applicationRepository.GetByIdAsync(applicationId);
            if (application == null)
            {
                return new ValidationResultDto
                {
                    IsValid = false,
                    ErrorMessage = $"Loan application with ID {applicationId} not found."
                };
            }

            var productResult = await ValidateProductAsync(application.ProductId);
            if (!productResult.IsValid)
            {
                result.IsValid = false;
                result.ErrorMessage = productResult.ErrorMessage;
                return result;
            }

            var amountResult = ValidateAmount(application.TotalAmount, 12);
            if (!amountResult.IsValid)
            {
                result.IsValid = false;
                result.ErrorMessage = amountResult.ErrorMessage;
                return result;
            }

            foreach (var warning in amountResult.Warnings)
            {
                result.Warnings.Add(warning);
            }

            return result;
        }

        public ValidationResultDto ValidateAmount(decimal amount, int termMonths)
        {
            var result = new ValidationResultDto { IsValid = true };

            if (amount <= 0)
            {
                result.IsValid = false;
                result.ErrorMessage = "Loan amount must be greater than zero.";
                return result;
            }

            if (termMonths <= 0)
            {
                result.IsValid = false;
                result.ErrorMessage = "Term months must be greater than zero.";
                return result;
            }

            if (amount > 1000000m)
            {
                result.Warnings.Add("Loan amount exceeds standard limit. Additional documentation may be required.");
            }

            if (termMonths > 360)
            {
                result.IsValid = false;
                result.ErrorMessage = "Term months cannot exceed 360 months.";
                return result;
            }

            return result;
        }

        public ValidationResultDto ValidateClientEligibility(string clientType, decimal annualIncome, decimal loanAmount)
        {
            var result = new ValidationResultDto { IsValid = true };

            if (string.IsNullOrWhiteSpace(clientType))
            {
                result.IsValid = false;
                result.ErrorMessage = "Client type is required.";
                return result;
            }

            if (annualIncome <= 0)
            {
                result.Warnings.Add("No income information provided. Loan may require additional verification.");
            }

            if (loanAmount > annualIncome * 10m && annualIncome > 0)
            {
                result.Warnings.Add("Loan amount exceeds 10 times annual income. High risk loan.");
            }

            return result;
        }

        public ValidationResultDto ValidateDebtToIncomeRatio(decimal monthlyDebtPayments, decimal monthlyIncome)
        {
            var result = new ValidationResultDto { IsValid = true };

            if (monthlyIncome <= 0)
            {
                result.IsValid = false;
                result.ErrorMessage = "Monthly income must be greater than zero.";
                return result;
            }

            var dtiRatio = monthlyDebtPayments / monthlyIncome;

            if (dtiRatio > 0.43m)
            {
                result.IsValid = false;
                result.ErrorMessage = $"Debt-to-income ratio ({dtiRatio:P2}) exceeds maximum allowed threshold of 43%.";
                return result;
            }

            if (dtiRatio > 0.36m)
            {
                result.Warnings.Add($"Debt-to-income ratio ({dtiRatio:P2}) is above recommended level of 36%.");
            }

            return result;
        }

        private async Task<ValidationResultDto> ValidateProductAsync(Guid productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                return new ValidationResultDto
                {
                    IsValid = false,
                    ErrorMessage = $"Loan product with ID {productId} not found."
                };
            }

            if (!product.IsActive)
            {
                return new ValidationResultDto
                {
                    IsValid = false,
                    ErrorMessage = "The selected loan product is currently inactive."
                };
            }

            return new ValidationResultDto { IsValid = true };
        }
    }
}
