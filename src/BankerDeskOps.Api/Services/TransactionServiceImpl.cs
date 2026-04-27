using BankerDeskOps.Api.Protos;
using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Application.Interfaces;
using Grpc.Core;
using Google.Protobuf.WellKnownTypes;

namespace BankerDeskOps.Api.Services
{
    public class TransactionServiceImpl : TransactionService.TransactionServiceBase
    {
        private readonly ITransactionService _transactionService;
        private readonly ILogger<TransactionServiceImpl> _logger;

        public TransactionServiceImpl(ITransactionService transactionService, ILogger<TransactionServiceImpl> logger)
        {
            _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task<GetAllTransactionsResponse> GetAllTransactions(Empty request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC: Fetching all transactions");
            var transactions = await _transactionService.GetAllAsync();
            var response = new GetAllTransactionsResponse();

            foreach (var transaction in transactions)
            {
                response.Transactions.Add(MapTransactionToProto(transaction));
            }

            return response;
        }

        public override async Task<GetTransactionByIdResponse> GetTransactionById(GetTransactionByIdRequest request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC: Fetching transaction with ID: {TransactionId}", request.Id);
            var transaction = await _transactionService.GetByIdAsync(Guid.Parse(request.Id));

            if (transaction is null)
            {
                _logger.LogWarning("gRPC: Transaction with ID {TransactionId} not found", request.Id);
                throw new RpcException(new Status(StatusCode.NotFound, $"Transaction with ID {request.Id} not found"));
            }

            return new GetTransactionByIdResponse { Transaction = MapTransactionToProto(transaction) };
        }

        public override async Task<TransferFundsResponse> TransferFunds(TransferFundsRequest request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC: Transferring {Amount} from {SourceAccountId} to {DestinationAccountId}",
                request.Amount, request.SourceAccountId, request.DestinationAccountId);

            try
            {
                var transferRequest = new TransferRequest
                {
                    FromAccountId = Guid.Parse(request.SourceAccountId),
                    ToAccountId = Guid.Parse(request.DestinationAccountId),
                    Amount = (decimal)request.Amount,
                    Description = request.Description
                };

                var createdTransaction = await _transactionService.ExecuteTransferAsync(transferRequest);
                return new TransferFundsResponse { Transaction = MapTransactionToProto(createdTransaction) };
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("gRPC: Transfer failed: {Message}", ex.Message);
                throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("gRPC: Invalid transfer request: {Message}", ex.Message);
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
        }

        private static Protos.TransactionDto MapTransactionToProto(Application.DTOs.TransactionDto transaction)
        {
            var proto = new Protos.TransactionDto
            {
                Id = transaction.Id.ToString(),
                TransactionType = MapTransactionType(transaction.TransactionType),
                Status = MapTransactionStatus(transaction.Status),
                ReferenceId = transaction.ReferenceId ?? string.Empty,
                CreatedAt = transaction.CreatedAt.ToString("o")
            };

            if (transaction.Entries != null)
            {
                foreach (var entry in transaction.Entries)
                {
                    proto.Entries.Add(MapEntryToProto(entry));
                }
            }

            return proto;
        }

        private static Protos.EntryDto MapEntryToProto(Application.DTOs.EntryDto entry)
        {
            return new Protos.EntryDto
            {
                Id = entry.Id.ToString(),
                TransactionId = entry.TransactionId.ToString(),
                AccountId = entry.AccountId.ToString(),
                Amount = (double)entry.Amount,
                EntryType = MapEntryType(entry.EntryType),
                BalanceAfter = (double)entry.BalanceAfter,
                Description = entry.Description ?? string.Empty,
                CreatedAt = entry.CreatedAt.ToString("o")
            };
        }

        private static Protos.TransactionType MapTransactionType(Domain.Enums.TransactionType type)
        {
            return type switch
            {
                Domain.Enums.TransactionType.Transfer => Protos.TransactionType.Transfer,
                _ => Protos.TransactionType.Unspecified
            };
        }

        private static Protos.TransactionStatus MapTransactionStatus(Domain.Enums.TransactionStatus status)
        {
            return status switch
            {
                Domain.Enums.TransactionStatus.Pending => Protos.TransactionStatus.Pending,
                Domain.Enums.TransactionStatus.Completed => Protos.TransactionStatus.Completed,
                Domain.Enums.TransactionStatus.Failed => Protos.TransactionStatus.Failed,
                Domain.Enums.TransactionStatus.Cancelled => Protos.TransactionStatus.Cancelled,
                _ => Protos.TransactionStatus.StatusUnspecified
            };
        }

        private static Protos.EntryType MapEntryType(Domain.Enums.EntryType type)
        {
            return type switch
            {
                Domain.Enums.EntryType.Debit => Protos.EntryType.Debit,
                Domain.Enums.EntryType.Credit => Protos.EntryType.Credit,
                _ => Protos.EntryType.Unspecified
            };
        }
    }
}
