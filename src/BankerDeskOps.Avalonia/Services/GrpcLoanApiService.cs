using BankerDeskOps.Api.Protos;
using BankerDeskOps.Application.DTOs;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;

namespace BankerDeskOps.Avalonia.Services;

/// <summary>
/// gRPC client wrapper for the Loan service endpoints.
/// </summary>
public sealed class GrpcLoanApiService
{
    private readonly GrpcChannelManager _channelManager;
    private readonly ILogger<GrpcLoanApiService> _logger;

    public GrpcLoanApiService(GrpcChannelManager channelManager, ILogger<GrpcLoanApiService> logger)
    {
        _channelManager = channelManager ?? throw new ArgumentNullException(nameof(channelManager));
        _logger         = logger         ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<LoanDto>> GetAllLoansAsync()
    {
        try
        {
            _logger.LogInformation("gRPC: Fetching all loans");
            var client   = new LoanService.LoanServiceClient(_channelManager.Channel);
            var response = await client.GetAllLoansAsync(new Empty());
            return response.Loans.Select(MapProtoToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError("gRPC: Error fetching loans: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<LoanDto?> GetLoanByIdAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("gRPC: Fetching loan {LoanId}", id);
            var client   = new LoanService.LoanServiceClient(_channelManager.Channel);
            var response = await client.GetLoanByIdAsync(new GetLoanByIdRequest { Id = id.ToString() });
            return response.Loan != null ? MapProtoToDto(response.Loan) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError("gRPC: Error fetching loan {LoanId}: {Message}", id, ex.Message);
            throw;
        }
    }

    public async Task<LoanDto?> CreateLoanAsync(Application.DTOs.CreateLoanRequest request)
    {
        try
        {
            _logger.LogInformation("gRPC: Creating loan for {CustomerName}", request.CustomerName);
            var client      = new LoanService.LoanServiceClient(_channelManager.Channel);
            var protoRequest = new Api.Protos.CreateLoanRequest
            {
                CustomerName = request.CustomerName,
                Amount       = (double)request.Amount,
                InterestRate = (double)request.InterestRate,
                TermMonths   = request.TermMonths
            };
            var response = await client.CreateLoanAsync(protoRequest);
            return response.Loan != null ? MapProtoToDto(response.Loan) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError("gRPC: Error creating loan: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<LoanDto?> ApproveLoanAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("gRPC: Approving loan {LoanId}", id);
            var client   = new LoanService.LoanServiceClient(_channelManager.Channel);
            var response = await client.ApproveLoanAsync(new ApproveLoanRequest { Id = id.ToString() });
            return response.Loan != null ? MapProtoToDto(response.Loan) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError("gRPC: Error approving loan: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<LoanDto?> RejectLoanAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("gRPC: Rejecting loan {LoanId}", id);
            var client   = new LoanService.LoanServiceClient(_channelManager.Channel);
            var response = await client.RejectLoanAsync(new RejectLoanRequest { Id = id.ToString() });
            return response.Loan != null ? MapProtoToDto(response.Loan) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError("gRPC: Error rejecting loan: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<LoanDto?> DisburseLoanAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("gRPC: Disbursing loan {LoanId}", id);
            var client   = new LoanService.LoanServiceClient(_channelManager.Channel);
            var response = await client.DisburseLoanAsync(new DisburseLoanRequest { Id = id.ToString() });
            return response.Loan != null ? MapProtoToDto(response.Loan) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError("gRPC: Error disbursing loan {LoanId}: {Message}", id, ex.Message);
            throw;
        }
    }

    public async Task<bool> DeleteLoanAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("gRPC: Deleting loan {LoanId}", id);
            var client   = new LoanService.LoanServiceClient(_channelManager.Channel);
            var response = await client.DeleteLoanAsync(new DeleteLoanRequest { Id = id.ToString() });
            return response.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError("gRPC: Error deleting loan: {Message}", ex.Message);
            throw;
        }
    }

    private static LoanDto MapProtoToDto(Api.Protos.Loan loan) => new()
    {
        Id           = Guid.Parse(loan.Id),
        CustomerName = loan.CustomerName,
        Amount       = (decimal)loan.Amount,
        InterestRate = (decimal)loan.InterestRate,
        TermMonths   = loan.TermMonths,
        Status       = (Domain.Enums.LoanStatus)(int)loan.Status,
        CreatedAt    = DateTime.Parse(loan.CreatedAt),
        UpdatedAt    = DateTime.Parse(loan.UpdatedAt)
    };
}
