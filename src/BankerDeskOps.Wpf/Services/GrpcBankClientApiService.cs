using BankerDeskOps.Api.Protos;
using BankerDeskOps.Application.DTOs;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;

namespace BankerDeskOps.Wpf.Services
{
    public class GrpcBankClientApiService
    {
        private readonly GrpcChannelManager _channelManager;
        private readonly ILogger<GrpcBankClientApiService> _logger;

        public GrpcBankClientApiService(GrpcChannelManager channelManager, ILogger<GrpcBankClientApiService> logger)
        {
            _channelManager = channelManager ?? throw new ArgumentNullException(nameof(channelManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<BankClientDto>> GetAllClientsAsync()
        {
            try
            {
                _logger.LogInformation("gRPC: Fetching all bank clients");
                var client = new BankClientService.BankClientServiceClient(_channelManager.Channel);
                var response = await client.GetAllClientsAsync(new Empty());
                return response.Clients.Select(MapProtoToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError("gRPC: Error fetching bank clients: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<BankClientDto?> GetClientByIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("gRPC: Fetching bank client {ClientId}", id);
                var client = new BankClientService.BankClientServiceClient(_channelManager.Channel);
                var response = await client.GetClientByIdAsync(new GetClientByIdRequest { Id = id.ToString() });
                return response.Client != null ? MapProtoToDto(response.Client) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError("gRPC: Error fetching bank client {ClientId}: {Message}", id, ex.Message);
                throw;
            }
        }

        public async Task<BankClientDto?> CreateClientAsync(Application.DTOs.CreateBankClientRequest request)
        {
            try
            {
                _logger.LogInformation("gRPC: Creating bank client {Email}", request.Email);
                var client = new BankClientService.BankClientServiceClient(_channelManager.Channel);
                var protoRequest = new Api.Protos.CreateBankClientRequest
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber ?? string.Empty,
                    DateOfBirth = request.DateOfBirth.ToString("yyyy-MM-dd"),
                    NationalId = request.NationalId,
                    Street = request.Street ?? string.Empty,
                    City = request.City ?? string.Empty,
                    PostalCode = request.PostalCode ?? string.Empty,
                    Country = request.Country ?? string.Empty
                };
                var response = await client.CreateClientAsync(protoRequest);
                return response.Client != null ? MapProtoToDto(response.Client) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError("gRPC: Error creating bank client: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<BankClientDto?> UpdateClientAsync(Guid id, Application.DTOs.UpdateBankClientRequest request)
        {
            try
            {
                _logger.LogInformation("gRPC: Updating bank client {ClientId}", id);
                var client = new BankClientService.BankClientServiceClient(_channelManager.Channel);
                var protoRequest = new Api.Protos.UpdateBankClientRequest
                {
                    Id = id.ToString(),
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber ?? string.Empty,
                    DateOfBirth = request.DateOfBirth.ToString("yyyy-MM-dd"),
                    NationalId = request.NationalId,
                    Street = request.Street ?? string.Empty,
                    City = request.City ?? string.Empty,
                    PostalCode = request.PostalCode ?? string.Empty,
                    Country = request.Country ?? string.Empty
                };
                var response = await client.UpdateClientAsync(protoRequest);
                return response.Client != null ? MapProtoToDto(response.Client) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError("gRPC: Error updating bank client: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<BankClientDto?> SuspendClientAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("gRPC: Suspending bank client {ClientId}", id);
                var client = new BankClientService.BankClientServiceClient(_channelManager.Channel);
                var response = await client.SuspendClientAsync(new SuspendClientRequest { Id = id.ToString() });
                return response.Client != null ? MapProtoToDto(response.Client) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError("gRPC: Error suspending bank client: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<BankClientDto?> ActivateClientAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("gRPC: Activating bank client {ClientId}", id);
                var client = new BankClientService.BankClientServiceClient(_channelManager.Channel);
                var response = await client.ActivateClientAsync(new ActivateClientRequest { Id = id.ToString() });
                return response.Client != null ? MapProtoToDto(response.Client) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError("gRPC: Error activating bank client: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<bool> DeleteClientAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("gRPC: Deleting bank client {ClientId}", id);
                var client = new BankClientService.BankClientServiceClient(_channelManager.Channel);
                var response = await client.DeleteClientAsync(new DeleteClientRequest { Id = id.ToString() });
                return response.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError("gRPC: Error deleting bank client: {Message}", ex.Message);
                throw;
            }
        }

        private static BankClientDto MapProtoToDto(Api.Protos.BankClient proto) => new BankClientDto
        {
            Id = Guid.Parse(proto.Id),
            FirstName = proto.FirstName,
            LastName = proto.LastName,
            Email = proto.Email,
            PhoneNumber = proto.PhoneNumber,
            DateOfBirth = DateTime.Parse(proto.DateOfBirth),
            NationalId = proto.NationalId,
            Street = proto.Street,
            City = proto.City,
            PostalCode = proto.PostalCode,
            Country = proto.Country,
            Status = (Domain.Enums.ClientStatus)(int)proto.Status,
            CreatedAt = DateTime.Parse(proto.CreatedAt),
            UpdatedAt = DateTime.Parse(proto.UpdatedAt)
        };
    }
}
