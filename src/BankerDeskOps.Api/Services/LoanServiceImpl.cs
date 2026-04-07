using BankerDeskOps.Api.Protos;
using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Application.Interfaces;
using Grpc.Core;
using Google.Protobuf.WellKnownTypes;

namespace BankerDeskOps.Api.Services
{
    /// <summary>
    /// gRPC service implementation for loan operations.
    /// </summary>
    public class LoanServiceImpl : LoanService.LoanServiceBase
    {
        private readonly ILoanService _loanService;
        private readonly ILogger<LoanServiceImpl> _logger;

        public LoanServiceImpl(ILoanService loanService, ILogger<LoanServiceImpl> logger)
        {
            _loanService = loanService ?? throw new ArgumentNullException(nameof(loanService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves all loans.
        /// </summary>
        public override async Task<GetAllLoansResponse> GetAllLoans(Empty request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC: Fetching all loans");
            var loans = await _loanService.GetAllAsync();
            var response = new GetAllLoansResponse();
            
            foreach (var loan in loans)
            {
                response.Loans.Add(MapLoanToProto(loan));
            }

            return response;
        }

        /// <summary>
        /// Retrieves a specific loan by ID.
        /// </summary>
        public override async Task<GetLoanByIdResponse> GetLoanById(GetLoanByIdRequest request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC: Fetching loan with ID: {LoanId}", request.Id);
            var loan = await _loanService.GetByIdAsync(Guid.Parse(request.Id));

            if (loan is null)
            {
                _logger.LogWarning("gRPC: Loan with ID {LoanId} not found", request.Id);
                throw new RpcException(new Status(StatusCode.NotFound, $"Loan with ID {request.Id} not found"));
            }

            return new GetLoanByIdResponse { Loan = MapLoanToProto(loan) };
        }

        /// <summary>
        /// Creates a new loan.
        /// </summary>
        public override async Task<CreateLoanResponse> CreateLoan(Protos.CreateLoanRequest request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC: Creating new loan for customer: {CustomerName}", request.CustomerName);
            
            try
            {
                var createRequest = new Application.DTOs.CreateLoanRequest
                {
                    CustomerName = request.CustomerName,
                    Amount = (decimal)request.Amount,
                    InterestRate = (decimal)request.InterestRate,
                    TermMonths = request.TermMonths
                };

                var createdLoan = await _loanService.CreateAsync(createRequest);
                return new CreateLoanResponse { Loan = MapLoanToProto(createdLoan) };
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("gRPC: Invalid loan creation request: {Message}", ex.Message);
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
        }

        /// <summary>
        /// Approves a loan.
        /// </summary>
        public override async Task<ApproveLoanResponse> ApproveLoan(ApproveLoanRequest request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC: Approving loan with ID: {LoanId}", request.Id);
            
            try
            {
                var updatedLoan = await _loanService.ApproveAsync(Guid.Parse(request.Id));
                return new ApproveLoanResponse { Loan = MapLoanToProto(updatedLoan) };
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("gRPC: Loan approval failed: {Message}", ex.Message);
                throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
            }
        }

        /// <summary>
        /// Rejects a loan.
        /// </summary>
        public override async Task<RejectLoanResponse> RejectLoan(RejectLoanRequest request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC: Rejecting loan with ID: {LoanId}", request.Id);
            
            try
            {
                var updatedLoan = await _loanService.RejectAsync(Guid.Parse(request.Id));
                return new RejectLoanResponse { Loan = MapLoanToProto(updatedLoan) };
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("gRPC: Loan rejection failed: {Message}", ex.Message);
                throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
            }
        }

        /// <summary>
        /// Deletes a loan.
        /// </summary>
        public override async Task<DeleteLoanResponse> DeleteLoan(DeleteLoanRequest request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC: Deleting loan with ID: {LoanId}", request.Id);
            var success = await _loanService.DeleteAsync(Guid.Parse(request.Id));
            return new DeleteLoanResponse { Success = success };
        }

        /// <summary>
        /// Maps a LoanDto to a Loan proto message.
        /// </summary>
        private static Protos.Loan MapLoanToProto(LoanDto loan)
        {
            return new Protos.Loan
            {
                Id = loan.Id.ToString(),
                CustomerName = loan.CustomerName,
                Amount = (double)loan.Amount,
                InterestRate = (double)loan.InterestRate,
                TermMonths = loan.TermMonths,
                Status = (LoanStatus)(int)loan.Status,
                CreatedAt = loan.CreatedAt.ToString("o"),
                UpdatedAt = loan.UpdatedAt.ToString("o")
            };
        }
    }
}
