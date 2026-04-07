using BankerDeskOps.Api.Protos;
using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Application.Interfaces;
using Grpc.Core;
using Google.Protobuf.WellKnownTypes;

namespace BankerDeskOps.Api.Services
{
    /// <summary>
    /// gRPC service implementation for retail account operations.
    /// </summary>
    public class RetailAccountServiceImpl : RetailAccountService.RetailAccountServiceBase
    {
        private readonly IRetailAccountService _accountService;
        private readonly ILogger<RetailAccountServiceImpl> _logger;

        public RetailAccountServiceImpl(IRetailAccountService accountService, ILogger<RetailAccountServiceImpl> logger)
        {
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves all retail accounts.
        /// </summary>
        public override async Task<GetAllAccountsResponse> GetAllAccounts(Empty request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC: Fetching all retail accounts");
            var accounts = await _accountService.GetAllAsync();
            var response = new GetAllAccountsResponse();
            
            foreach (var account in accounts)
            {
                response.Accounts.Add(MapAccountToProto(account));
            }

            return response;
        }

        /// <summary>
        /// Retrieves a specific account by ID.
        /// </summary>
        public override async Task<GetAccountByIdResponse> GetAccountById(GetAccountByIdRequest request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC: Fetching account with ID: {AccountId}", request.Id);
            var account = await _accountService.GetByIdAsync(Guid.Parse(request.Id));

            if (account is null)
            {
                _logger.LogWarning("gRPC: Account with ID {AccountId} not found", request.Id);
                throw new RpcException(new Status(StatusCode.NotFound, $"Account with ID {request.Id} not found"));
            }

            return new GetAccountByIdResponse { Account = MapAccountToProto(account) };
        }

        /// <summary>
        /// Opens a new retail account.
        /// </summary>
        public override async Task<OpenAccountResponse> OpenAccount(OpenAccountRequest request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC: Opening new account for customer: {CustomerName}", request.CustomerName);
            
            try
            {
                var openRequest = new CreateRetailAccountRequest
                {
                    CustomerName = request.CustomerName,
                    AccountType = (Domain.Enums.AccountType)(int)request.AccountType,
                    InitialDeposit = (decimal)request.InitialDeposit
                };

                var createdAccount = await _accountService.OpenAsync(openRequest);
                return new OpenAccountResponse { Account = MapAccountToProto(createdAccount) };
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("gRPC: Invalid account creation request: {Message}", ex.Message);
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
        }

        /// <summary>
        /// Deposits funds into an account.
        /// </summary>
        public override async Task<DepositResponse> Deposit(Protos.DepositRequest request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC: Depositing {Amount} into account {AccountId}", request.Amount, request.AccountId);
            
            try
            {
                var depositRequest = new Application.DTOs.DepositRequest
                {
                    Amount = (decimal)request.Amount
                };

                var updatedAccount = await _accountService.DepositAsync(Guid.Parse(request.AccountId), depositRequest);
                return new DepositResponse { Account = MapAccountToProto(updatedAccount) };
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("gRPC: Deposit failed: {Message}", ex.Message);
                throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("gRPC: Invalid deposit request: {Message}", ex.Message);
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
        }

        /// <summary>
        /// Withdraws funds from an account.
        /// </summary>
        public override async Task<WithdrawResponse> Withdraw(Protos.WithdrawRequest request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC: Withdrawing {Amount} from account {AccountId}", request.Amount, request.AccountId);
            
            try
            {
                var withdrawRequest = new Application.DTOs.WithdrawRequest
                {
                    Amount = (decimal)request.Amount
                };

                var updatedAccount = await _accountService.WithdrawAsync(Guid.Parse(request.AccountId), withdrawRequest);
                return new WithdrawResponse { Account = MapAccountToProto(updatedAccount) };
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("gRPC: Withdrawal failed: {Message}", ex.Message);
                throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("gRPC: Invalid withdrawal request: {Message}", ex.Message);
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
        }

        /// <summary>
        /// Closes a retail account.
        /// </summary>
        public override async Task<CloseAccountResponse> CloseAccount(CloseAccountRequest request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC: Closing account with ID: {AccountId}", request.Id);
            var success = await _accountService.CloseAsync(Guid.Parse(request.Id));
            return new CloseAccountResponse { Success = success };
        }

        /// <summary>
        /// Maps a RetailAccountDto to an account proto message.
        /// </summary>
        private static Protos.RetailAccount MapAccountToProto(RetailAccountDto account)
        {
            return new Protos.RetailAccount
            {
                Id = account.Id.ToString(),
                CustomerName = account.CustomerName,
                AccountNumber = account.AccountNumber,
                Balance = (double)account.Balance,
                AccountType = (AccountType)(int)account.AccountType,
                OpenedAt = account.OpenedAt.ToString("o"),
                UpdatedAt = account.UpdatedAt.ToString("o")
            };
        }
    }
}
