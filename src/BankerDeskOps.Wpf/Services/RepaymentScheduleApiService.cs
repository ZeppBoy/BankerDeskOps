using BankerDeskOps.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace BankerDeskOps.Wpf.Services
{
    public class RepaymentScheduleApiService
    {
        private readonly ApiClient _apiClient;
        private readonly ILogger<RepaymentScheduleApiService> _logger;
        private const string Endpoint = "api/repaymentschedules";

        public RepaymentScheduleApiService(ApiClient apiClient, ILogger<RepaymentScheduleApiService> logger)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<RepaymentScheduleDto>> GetAllSchedulesAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all repayment schedules");
                var schedules = await _apiClient.GetAsync<IEnumerable<RepaymentScheduleDto>>(Endpoint);
                return schedules ?? Enumerable.Empty<RepaymentScheduleDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching repayment schedules: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<RepaymentScheduleDto>> GetSchedulesByLoanApplicationAsync(Guid loanApplicationId)
        {
            try
            {
                _logger.LogInformation("Fetching repayment schedules for loan application {LoanApplicationId}", loanApplicationId);
                var schedules = await _apiClient.GetAsync<IEnumerable<RepaymentScheduleDto>>($"{Endpoint}/by-application/{loanApplicationId}");
                return schedules ?? Enumerable.Empty<RepaymentScheduleDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching repayment schedules for loan application {LoanApplicationId}: {Message}", loanApplicationId, ex.Message);
                throw;
            }
        }

        public async Task<RepaymentScheduleDto?> CreateScheduleAsync(CreateRepaymentScheduleRequest request)
        {
            try
            {
                _logger.LogInformation("Creating repayment schedule for application {LoanApplicationId}", request.LoanApplicationId);
                return await _apiClient.PostAsync<CreateRepaymentScheduleRequest, RepaymentScheduleDto>(Endpoint, request);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating repayment schedule: {Message}", ex.Message);
                throw;
            }
        }

        public async Task DeleteScheduleAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting repayment schedule {ScheduleId}", id);
                await _apiClient.DeleteAsync($"{Endpoint}/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error deleting repayment schedule {ScheduleId}: {Message}", id, ex.Message);
                throw;
            }
        }
    }
}
