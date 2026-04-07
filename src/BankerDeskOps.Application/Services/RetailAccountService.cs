using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Application.Interfaces;
using BankerDeskOps.Domain.Entities;

namespace BankerDeskOps.Application.Services
{
    /// <summary>
    /// Service implementation for retail account business logic.
    /// </summary>
    public class RetailAccountService : IRetailAccountService
    {
        private readonly IRetailAccountRepository _accountRepository;

        public RetailAccountService(IRetailAccountRepository accountRepository)
        {
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        }

        public async Task<IEnumerable<RetailAccountDto>> GetAllAsync()
        {
            var accounts = await _accountRepository.GetAllAsync();
            return accounts.Select(MapToDto);
        }

        public async Task<RetailAccountDto?> GetByIdAsync(Guid id)
        {
            var account = await _accountRepository.GetByIdAsync(id);
            return account is null ? null : MapToDto(account);
        }

        public async Task<RetailAccountDto> OpenAsync(CreateRetailAccountRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));
            
            if (string.IsNullOrWhiteSpace(request.CustomerName))
                throw new ArgumentException("Customer name cannot be empty.", nameof(request.CustomerName));

            var account = new RetailAccount
            {
                Id = Guid.NewGuid(),
                CustomerName = request.CustomerName,
                AccountNumber = GenerateAccountNumber(),
                Balance = request.InitialDeposit,
                AccountType = request.AccountType,
                OpenedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdAccount = await _accountRepository.CreateAsync(account);
            return MapToDto(createdAccount);
        }

        public async Task<RetailAccountDto> DepositAsync(Guid id, DepositRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            if (request.Amount <= 0)
                throw new InvalidOperationException("Deposit amount must be greater than zero.");

            var account = await _accountRepository.GetByIdAsync(id);
            if (account is null)
                throw new InvalidOperationException($"Account with ID {id} not found.");

            account.Balance += request.Amount;
            account.UpdatedAt = DateTime.UtcNow;

            var updatedAccount = await _accountRepository.UpdateAsync(account);
            return MapToDto(updatedAccount);
        }

        public async Task<RetailAccountDto> WithdrawAsync(Guid id, WithdrawRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            if (request.Amount <= 0)
                throw new InvalidOperationException("Withdrawal amount must be greater than zero.");

            var account = await _accountRepository.GetByIdAsync(id);
            if (account is null)
                throw new InvalidOperationException($"Account with ID {id} not found.");

            if (account.Balance < request.Amount)
                throw new InvalidOperationException($"Insufficient funds. Available balance: {account.Balance}");

            account.Balance -= request.Amount;
            account.UpdatedAt = DateTime.UtcNow;

            var updatedAccount = await _accountRepository.UpdateAsync(account);
            return MapToDto(updatedAccount);
        }

        public async Task<bool> CloseAsync(Guid id)
        {
            return await _accountRepository.DeleteAsync(id);
        }

        private static RetailAccountDto MapToDto(RetailAccount account)
        {
            return new RetailAccountDto
            {
                Id = account.Id,
                CustomerName = account.CustomerName,
                AccountNumber = account.AccountNumber,
                Balance = account.Balance,
                AccountType = account.AccountType,
                OpenedAt = account.OpenedAt,
                UpdatedAt = account.UpdatedAt
            };
        }

        private static string GenerateAccountNumber()
        {
            // Generate a 10-digit account number (format: XXXXXXXXXX)
            return DateTime.UtcNow.Ticks.ToString().Substring(0, 10);
        }
    }
}
