using BankerDeskOps.Api.Protos;
using BankerDeskOps.Application.DTOs;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;

namespace BankerDeskOps.Avalonia.Services;

/// <summary>
/// gRPC client wrapper for the Retail Account service endpoints.
/// </summary>
public sealed class GrpcRetailAccountApiService
{
    private readonly GrpcChannelManager _channelManager;
    private readonly ILogger<GrpcRetailAccountApiService> _logger;

    public GrpcRetailAccountApiService(GrpcChannelManager channelManager, ILogger<GrpcRetailAccountApiService> logger)
    {
        _channelManager = channelManager ?? throw new ArgumentNullException(nameof(channelManager));
        _logger         = logger         ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<RetailAccountDto>> GetAllAccountsAsync()
    {
        try
        {
            _logger.LogInformation("gRPC: Fetching all retail accounts");
            var client   = new RetailAccountService.RetailAccountServiceClient(_channelManager.Channel);
            var response = await client.GetAllAccountsAsync(new Empty());
            return response.Accounts.Select(MapProtoToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError("gRPC: Error fetching accounts: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<RetailAccountDto?> GetAccountByIdAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("gRPC: Fetching account {AccountId}", id);
            var client   = new RetailAccountService.RetailAccountServiceClient(_channelManager.Channel);
            var response = await client.GetAccountByIdAsync(new GetAccountByIdRequest { Id = id.ToString() });
            return response.Account != null ? MapProtoToDto(response.Account) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError("gRPC: Error fetching account {AccountId}: {Message}", id, ex.Message);
            throw;
        }
    }

    public async Task<RetailAccountDto?> OpenAccountAsync(CreateRetailAccountRequest request)
    {
        try
        {
            _logger.LogInformation("gRPC: Opening account for {CustomerName}", request.CustomerName);
            var client      = new RetailAccountService.RetailAccountServiceClient(_channelManager.Channel);
            var protoRequest = new OpenAccountRequest
            {
                CustomerName  = request.CustomerName,
                AccountType   = (Api.Protos.AccountType)(int)request.AccountType,
                InitialDeposit = (double)request.InitialDeposit
            };
            var response = await client.OpenAccountAsync(protoRequest);
            return response.Account != null ? MapProtoToDto(response.Account) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError("gRPC: Error opening account: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<RetailAccountDto?> DepositAsync(Guid accountId, Application.DTOs.DepositRequest request)
    {
        try
        {
            _logger.LogInformation("gRPC: Depositing {Amount} into account {AccountId}", request.Amount, accountId);
            var client      = new RetailAccountService.RetailAccountServiceClient(_channelManager.Channel);
            var protoRequest = new Api.Protos.DepositRequest
            {
                AccountId = accountId.ToString(),
                Amount    = (double)request.Amount
            };
            var response = await client.DepositAsync(protoRequest);
            return response.Account != null ? MapProtoToDto(response.Account) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError("gRPC: Error depositing funds: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<RetailAccountDto?> WithdrawAsync(Guid accountId, Application.DTOs.WithdrawRequest request)
    {
        try
        {
            _logger.LogInformation("gRPC: Withdrawing {Amount} from account {AccountId}", request.Amount, accountId);
            var client      = new RetailAccountService.RetailAccountServiceClient(_channelManager.Channel);
            var protoRequest = new Api.Protos.WithdrawRequest
            {
                AccountId = accountId.ToString(),
                Amount    = (double)request.Amount
            };
            var response = await client.WithdrawAsync(protoRequest);
            return response.Account != null ? MapProtoToDto(response.Account) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError("gRPC: Error withdrawing funds: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<bool> CloseAccountAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("gRPC: Closing account {AccountId}", id);
            var client   = new RetailAccountService.RetailAccountServiceClient(_channelManager.Channel);
            var response = await client.CloseAccountAsync(new CloseAccountRequest { Id = id.ToString() });
            return response.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError("gRPC: Error closing account: {Message}", ex.Message);
            throw;
        }
    }

    private static RetailAccountDto MapProtoToDto(Api.Protos.RetailAccount account) => new()
    {
        Id            = Guid.Parse(account.Id),
        CustomerName  = account.CustomerName,
        AccountNumber = account.AccountNumber,
        Iban          = account.Iban,
        Balance       = (decimal)account.Balance,
        AccountType   = (Domain.Enums.AccountType)(int)account.AccountType,
        OpenedAt      = DateTime.Parse(account.OpenedAt),
        UpdatedAt     = DateTime.Parse(account.UpdatedAt)
    };
}
