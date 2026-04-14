using BankerDeskOps.Api.Protos;
using BankerDeskOps.Application.Interfaces;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace BankerDeskOps.Api.Services
{
    /// <summary>
    /// gRPC service implementation for contract read operations.
    /// </summary>
    public class ContractServiceImpl : ContractService.ContractServiceBase
    {
        private readonly IContractService _contractService;
        private readonly ILogger<ContractServiceImpl> _logger;

        public ContractServiceImpl(IContractService contractService, ILogger<ContractServiceImpl> logger)
        {
            _contractService = contractService ?? throw new ArgumentNullException(nameof(contractService));
            _logger          = logger          ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task<GetAllContractsResponse> GetAllContracts(
            Empty request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC: Fetching all contracts");
            var contracts = await _contractService.GetAllAsync();
            var response  = new GetAllContractsResponse();
            foreach (var c in contracts)
                response.Contracts.Add(MapToProto(c));
            return response;
        }

        public override async Task<GetContractByIdResponse> GetContractById(
            GetContractByIdRequest request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC: Fetching contract {ContractId}", request.Id);
            var contract = await _contractService.GetByIdAsync(Guid.Parse(request.Id));

            if (contract is null)
                throw new RpcException(new Status(StatusCode.NotFound,
                    $"Contract with ID {request.Id} not found"));

            return new GetContractByIdResponse { Contract = MapToProto(contract) };
        }

        public override async Task<GetContractByLoanIdResponse> GetContractByLoanId(
            GetContractByLoanIdRequest request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC: Fetching contract for loan {LoanId}", request.LoanId);
            var contract = await _contractService.GetByLoanIdAsync(Guid.Parse(request.LoanId));

            if (contract is null)
                throw new RpcException(new Status(StatusCode.NotFound,
                    $"No contract found for loan {request.LoanId}"));

            return new GetContractByLoanIdResponse { Contract = MapToProto(contract) };
        }

        private static Protos.Contract MapToProto(Application.DTOs.ContractDto c) => new()
        {
            Id             = c.Id.ToString(),
            ContractNumber = c.ContractNumber,
            LoanId         = c.LoanId.ToString(),
            CustomerName   = c.CustomerName,
            LoanAmount     = (double)c.LoanAmount,
            InterestRate   = (double)c.InterestRate,
            TermMonths     = c.TermMonths,
            DisbursedAt    = c.DisbursedAt.ToString("o"),
            Status         = (ContractStatus)(int)c.Status,
            CreatedAt      = c.CreatedAt.ToString("o"),
            UpdatedAt      = c.UpdatedAt.ToString("o")
        };
    }
}
