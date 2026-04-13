using BankerDeskOps.Api.Protos;
using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Application.Interfaces;
using Grpc.Core;
using Google.Protobuf.WellKnownTypes;

namespace BankerDeskOps.Api.Services
{
    public class BankClientServiceImpl : BankClientService.BankClientServiceBase
    {
        private readonly IBankClientService _clientService;
        private readonly ILogger<BankClientServiceImpl> _logger;

        public BankClientServiceImpl(IBankClientService clientService, ILogger<BankClientServiceImpl> logger)
        {
            _clientService = clientService ?? throw new ArgumentNullException(nameof(clientService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task<GetAllClientsResponse> GetAllClients(Empty request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC: Fetching all bank clients");
            var clients = await _clientService.GetAllAsync();
            var response = new GetAllClientsResponse();
            foreach (var client in clients)
                response.Clients.Add(MapClientToProto(client));
            return response;
        }

        public override async Task<GetClientByIdResponse> GetClientById(GetClientByIdRequest request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC: Fetching bank client with ID: {ClientId}", request.Id);
            var client = await _clientService.GetByIdAsync(Guid.Parse(request.Id));

            if (client is null)
            {
                _logger.LogWarning("gRPC: Bank client with ID {ClientId} not found", request.Id);
                throw new RpcException(new Status(StatusCode.NotFound, $"Bank client with ID {request.Id} not found"));
            }

            return new GetClientByIdResponse { Client = MapClientToProto(client) };
        }

        public override async Task<CreateBankClientResponse> CreateClient(Protos.CreateBankClientRequest request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC: Creating bank client: {Email}", request.Email);

            try
            {
                var createRequest = new Application.DTOs.CreateBankClientRequest
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    DateOfBirth = DateTime.Parse(request.DateOfBirth),
                    NationalId = request.NationalId,
                    Street = request.Street,
                    City = request.City,
                    PostalCode = request.PostalCode,
                    Country = request.Country
                };

                var created = await _clientService.CreateAsync(createRequest);
                return new CreateBankClientResponse { Client = MapClientToProto(created) };
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("gRPC: Invalid bank client request: {Message}", ex.Message);
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("gRPC: Bank client conflict: {Message}", ex.Message);
                throw new RpcException(new Status(StatusCode.AlreadyExists, ex.Message));
            }
        }

        public override async Task<UpdateBankClientResponse> UpdateClient(Protos.UpdateBankClientRequest request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC: Updating bank client with ID: {ClientId}", request.Id);

            try
            {
                var updateRequest = new Application.DTOs.UpdateBankClientRequest
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    DateOfBirth = DateTime.Parse(request.DateOfBirth),
                    NationalId = request.NationalId,
                    Street = request.Street,
                    City = request.City,
                    PostalCode = request.PostalCode,
                    Country = request.Country
                };

                var updated = await _clientService.UpdateAsync(Guid.Parse(request.Id), updateRequest);
                return new UpdateBankClientResponse { Client = MapClientToProto(updated) };
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("gRPC: Invalid bank client update: {Message}", ex.Message);
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("gRPC: Bank client update failed: {Message}", ex.Message);
                throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
            }
        }

        public override async Task<SuspendClientResponse> SuspendClient(SuspendClientRequest request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC: Suspending bank client with ID: {ClientId}", request.Id);

            try
            {
                var updated = await _clientService.SuspendAsync(Guid.Parse(request.Id));
                return new SuspendClientResponse { Client = MapClientToProto(updated) };
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("gRPC: Suspend client failed: {Message}", ex.Message);
                throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
            }
        }

        public override async Task<ActivateClientResponse> ActivateClient(ActivateClientRequest request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC: Activating bank client with ID: {ClientId}", request.Id);

            try
            {
                var updated = await _clientService.ActivateAsync(Guid.Parse(request.Id));
                return new ActivateClientResponse { Client = MapClientToProto(updated) };
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("gRPC: Activate client failed: {Message}", ex.Message);
                throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
            }
        }

        public override async Task<DeleteClientResponse> DeleteClient(DeleteClientRequest request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC: Deleting bank client with ID: {ClientId}", request.Id);
            var success = await _clientService.DeleteAsync(Guid.Parse(request.Id));
            return new DeleteClientResponse { Success = success };
        }

        private static Protos.BankClient MapClientToProto(BankClientDto client) => new Protos.BankClient
        {
            Id = client.Id.ToString(),
            FirstName = client.FirstName,
            LastName = client.LastName,
            Email = client.Email,
            PhoneNumber = client.PhoneNumber,
            DateOfBirth = client.DateOfBirth.ToString("yyyy-MM-dd"),
            NationalId = client.NationalId,
            Street = client.Street,
            City = client.City,
            PostalCode = client.PostalCode,
            Country = client.Country,
            Status = (ClientStatus)(int)client.Status,
            CreatedAt = client.CreatedAt.ToString("o"),
            UpdatedAt = client.UpdatedAt.ToString("o")
        };
    }
}
