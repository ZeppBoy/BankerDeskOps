using BankerDeskOps.Api.Protos;
using BankerDeskOps.Application.DTOs;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;

namespace BankerDeskOps.Wpf.Services
{
    /// <summary>
    /// gRPC service for interacting with transaction API endpoints.
    /// </summary>
    public class GrpcTransactionApiService
    {
        private readonly GrpcChannelManager _channelManager;
        private readonly ILogger<GrpcTransactionApiService> _logger;

        public GrpcTransactionApiService(GrpcChannelManager channelManager, ILogger<GrpcTransactionApiService> logger)
        {
            _channelManager = channelManager ?? throw new ArgumentNullException(nameof(channelManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves all transactions.
        /// </summary>
        public async Task<IEnumerable<TransactionDto>> GetAllTransactionsAsync()
        {
            try
            {
                _logger.LogInformation("gRPC: Fetching all transactions");
                var client = new TransactionService.TransactionServiceClient(_channelManager.Channel);
                var response = await client.GetAllTransactionsAsync(new Empty());
                return response.Transactions.Select<Api.Protos.Transaction, TransactionDto>(MapProtoToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError("gRPC: Error fetching transactions: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Retrieves a transaction by ID.
        /// </summary>
        public async Task<TransactionDto?> GetTransactionByIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("gRPC: Fetching transaction {TransactionId}", id);
                var client = new TransactionService.TransactionServiceClient(_channelManager.Channel);
                var response = await client.GetTransactionByIdAsync(new GetTransactionByIdRequest { Id = id.ToString() });
                return response.Transaction != null ? MapProtoToDto(response.Transaction) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError("gRPC: Error fetching transaction {TransactionId}: {Message}", id, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Executes a fund transfer between two accounts.
        /// </summary>
        public async Task<TransactionDto?> TransferFundsAsync(TransferRequest request)
        {
            try
            {
                _logger.LogInformation("gRPC: Transferring {Amount} from account {FromAccountId} to {ToAccountId}",
                    request.Amount, request.FromAccountId, request.ToAccountId);
                var client = new TransactionService.TransactionServiceClient(_channelManager.Channel);
                var protoRequest = new Api.Protos.TransferFundsRequest
                {
                    SourceAccountId = request.FromAccountId.ToString(),
                    DestinationAccountId = request.ToAccountId.ToString(),
                    Amount = (double)request.Amount,
                    Description = request.Description ?? string.Empty
                };
                var response = await client.TransferFundsAsync(protoRequest);
                return response.Transaction != null ? MapProtoToDto(response.Transaction) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError("gRPC: Error transferring funds: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Maps a proto TransactionDto message to an Application DTO.
        /// </summary>
        private static TransactionDto MapProtoToDto(Api.Protos.Transaction transaction)
        {
            return new TransactionDto
            {
                Id = Guid.Parse(transaction.Id),
                TransactionType = (Domain.Enums.TransactionType)(int)transaction.TransactionType,
                Status = (Domain.Enums.TransactionStatus)(int)transaction.Status,
                ReferenceId = string.IsNullOrEmpty(transaction.ReferenceId) ? null : transaction.ReferenceId,
                CreatedAt = DateTime.Parse(transaction.CreatedAt),
                Entries = transaction.Entries.Select(MapEntryProtoToDto).ToList()
            };
        }

        /// <summary>
        /// Maps a proto EntryDto message to an Application DTO.
        /// </summary>
        private static EntryDto MapEntryProtoToDto(Api.Protos.Entry entry)
        {
            return new EntryDto
            {
                Id = Guid.Parse(entry.Id),
                AccountId = Guid.Parse(entry.AccountId),
                Amount = (decimal)entry.Amount,
                EntryType = (Domain.Enums.EntryType)(int)entry.EntryType,
                BalanceAfter = (decimal)entry.BalanceAfter,
                Description = string.IsNullOrEmpty(entry.Description) ? null : entry.Description,
                CreatedAt = DateTime.Parse(entry.CreatedAt)
            };
        }
    }
}
