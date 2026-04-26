using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Application.Interfaces;
using BankerDeskOps.Domain.Entities;
using BankerDeskOps.Domain.Enums;

namespace BankerDeskOps.Application.Services
{
    /// <summary>
    /// Service implementation for transaction business logic.
    /// Handles atomic transfers with double-entry bookkeeping.
    /// </summary>
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IRetailAccountRepository _accountRepository;
        private readonly IEntryRepository _entryRepository;

        public TransactionService(
            ITransactionRepository transactionRepository,
            IRetailAccountRepository accountRepository,
            IEntryRepository entryRepository)
        {
            _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
            _entryRepository = entryRepository ?? throw new ArgumentNullException(nameof(entryRepository));
        }

        public async Task<IEnumerable<TransactionDto>> GetAllAsync()
        {
            var transactions = await _transactionRepository.GetAllAsync();
            return transactions.Select(MapToDto);
        }

        public async Task<TransactionDto?> GetByIdAsync(Guid id)
        {
            var transaction = await _transactionRepository.GetByIdAsync(id);
            return transaction is null ? null : MapToDto(transaction);
        }

        /// <summary>
        /// Executes an atomic money transfer between two accounts.
        /// Uses double-entry bookkeeping: one debit (source) and one credit (destination).
        /// </summary>
        public async Task<TransactionDto> ExecuteTransferAsync(TransferRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            if (request.Amount <= 0)
                throw new ArgumentException("Transfer amount must be greater than zero.", nameof(request.Amount));

            if (request.FromAccountId == request.ToAccountId)
                throw new ArgumentException("Source and destination accounts cannot be the same.");

            // Fetch both accounts within the same unit of work context
            var sourceAccount = await _accountRepository.GetByIdAsync(request.FromAccountId);
            if (sourceAccount is null)
                throw new InvalidOperationException($"Source account with ID {request.FromAccountId} not found.");

            var destinationAccount = await _accountRepository.GetByIdAsync(request.ToAccountId);
            if (destinationAccount is null)
                throw new InvalidOperationException($"Destination account with ID {request.ToAccountId} not found.");

            // Validate sufficient funds before proceeding
            if (sourceAccount.Balance < request.Amount)
                throw new InvalidOperationException($"Insufficient funds. Available balance: {sourceAccount.Balance}");

            var transactionId = Guid.NewGuid();
            var referenceId = GenerateReferenceId();
            var now = DateTime.UtcNow;

            // Create the transaction record
            var transaction = new Transaction
            {
                Id = transactionId,
                TransactionType = TransactionType.Transfer,
                Status = TransactionStatus.Pending,
                ReferenceId = referenceId,
                CreatedAt = now
            };

            // Create debit entry (source account loses money)
            var debitEntry = new Entry
            {
                Id = Guid.NewGuid(),
                TransactionId = transactionId,
                AccountId = request.FromAccountId,
                Amount = request.Amount,
                EntryType = EntryType.Debit,
                BalanceAfter = sourceAccount.Balance - request.Amount,
                Description = request.Description ?? $"Transfer to account {destinationAccount.AccountNumber}",
                CreatedAt = now
            };

            // Create credit entry (destination account gains money)
            var creditEntry = new Entry
            {
                Id = Guid.NewGuid(),
                TransactionId = transactionId,
                AccountId = request.ToAccountId,
                Amount = request.Amount,
                EntryType = EntryType.Credit,
                BalanceAfter = destinationAccount.Balance + request.Amount,
                Description = request.Description ?? $"Transfer from account {sourceAccount.AccountNumber}",
                CreatedAt = now
            };

            // Persist transaction with entries atomically
            var createdTransaction = await _transactionRepository.CreateWithEntriesAsync(transaction, new List<Entry> { debitEntry, creditEntry });

            // Update source account balance (debit)
            sourceAccount.Balance -= request.Amount;
            sourceAccount.UpdatedAt = DateTime.UtcNow;
            await _accountRepository.UpdateAsync(sourceAccount);

            // Update destination account balance (credit)
            destinationAccount.Balance += request.Amount;
            destinationAccount.UpdatedAt = DateTime.UtcNow;
            await _accountRepository.UpdateAsync(destinationAccount);

            // Mark transaction as completed
            await _transactionRepository.UpdateStatusAsync(transactionId, TransactionStatus.Completed);

            var finalTransaction = await _transactionRepository.GetByIdAsync(transactionId);
            return MapToDto(finalTransaction!);
        }

        public async Task<IEnumerable<EntryDto>> GetEntriesByAccountIdAsync(Guid accountId)
        {
            var entries = await _entryRepository.GetByAccountIdAsync(accountId);
            return entries.Select(MapEntryToDto);
        }

        private static TransactionDto MapToDto(Transaction transaction)
        {
            return new TransactionDto
            {
                Id = transaction.Id,
                TransactionType = transaction.TransactionType,
                Status = transaction.Status,
                ReferenceId = transaction.ReferenceId,
                CreatedAt = transaction.CreatedAt,
                Entries = transaction.Entries?.Select(MapEntryToDto).ToList() ?? new List<EntryDto>()
            };
        }

        private static EntryDto MapEntryToDto(Entry entry)
        {
            return new EntryDto
            {
                Id = entry.Id,
                TransactionId = entry.TransactionId,
                AccountId = entry.AccountId,
                Amount = entry.Amount,
                EntryType = entry.EntryType,
                BalanceAfter = entry.BalanceAfter,
                Description = entry.Description,
                CreatedAt = entry.CreatedAt
            };
        }

        private static string GenerateReferenceId()
        {
            return $"TXN-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        }
    }
}