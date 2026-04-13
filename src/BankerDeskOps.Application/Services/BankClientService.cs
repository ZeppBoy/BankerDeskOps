using System.Net.Mail;
using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Application.Interfaces;
using BankerDeskOps.Domain.Entities;
using BankerDeskOps.Domain.Enums;

namespace BankerDeskOps.Application.Services
{
    public class BankClientService : IBankClientService
    {
        private readonly IBankClientRepository _clientRepository;

        public BankClientService(IBankClientRepository clientRepository)
        {
            _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
        }

        public async Task<IEnumerable<BankClientDto>> GetAllAsync()
        {
            var clients = await _clientRepository.GetAllAsync();
            return clients.Select(MapToDto);
        }

        public async Task<BankClientDto?> GetByIdAsync(Guid id)
        {
            var client = await _clientRepository.GetByIdAsync(id);
            return client is null ? null : MapToDto(client);
        }

        public async Task<BankClientDto> CreateAsync(CreateBankClientRequest request)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));
            ValidateName(request.FirstName, nameof(request.FirstName));
            ValidateName(request.LastName, nameof(request.LastName));
            ValidateEmail(request.Email);

            var existingByEmail = await _clientRepository.GetByEmailAsync(request.Email);
            if (existingByEmail is not null)
                throw new InvalidOperationException($"A client with email '{request.Email}' already exists.");

            var existingByNationalId = await _clientRepository.GetByNationalIdAsync(request.NationalId);
            if (existingByNationalId is not null)
                throw new InvalidOperationException($"A client with national ID '{request.NationalId}' already exists.");

            var client = new BankClient
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Email = request.Email.Trim().ToLowerInvariant(),
                PhoneNumber = request.PhoneNumber?.Trim() ?? string.Empty,
                DateOfBirth = request.DateOfBirth.Date,
                NationalId = request.NationalId.Trim(),
                Street = request.Street?.Trim() ?? string.Empty,
                City = request.City?.Trim() ?? string.Empty,
                PostalCode = request.PostalCode?.Trim() ?? string.Empty,
                Country = request.Country?.Trim() ?? string.Empty,
                Status = ClientStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _clientRepository.CreateAsync(client);
            return MapToDto(created);
        }

        public async Task<BankClientDto> UpdateAsync(Guid id, UpdateBankClientRequest request)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));
            ValidateName(request.FirstName, nameof(request.FirstName));
            ValidateName(request.LastName, nameof(request.LastName));
            ValidateEmail(request.Email);

            var client = await _clientRepository.GetByIdAsync(id);
            if (client is null)
                throw new InvalidOperationException($"Bank client with ID {id} not found.");

            var emailOwner = await _clientRepository.GetByEmailAsync(request.Email);
            if (emailOwner is not null && emailOwner.Id != id)
                throw new InvalidOperationException($"A client with email '{request.Email}' already exists.");

            var nationalIdOwner = await _clientRepository.GetByNationalIdAsync(request.NationalId);
            if (nationalIdOwner is not null && nationalIdOwner.Id != id)
                throw new InvalidOperationException($"A client with national ID '{request.NationalId}' already exists.");

            client.FirstName = request.FirstName.Trim();
            client.LastName = request.LastName.Trim();
            client.Email = request.Email.Trim().ToLowerInvariant();
            client.PhoneNumber = request.PhoneNumber?.Trim() ?? string.Empty;
            client.DateOfBirth = request.DateOfBirth.Date;
            client.NationalId = request.NationalId.Trim();
            client.Street = request.Street?.Trim() ?? string.Empty;
            client.City = request.City?.Trim() ?? string.Empty;
            client.PostalCode = request.PostalCode?.Trim() ?? string.Empty;
            client.Country = request.Country?.Trim() ?? string.Empty;
            client.UpdatedAt = DateTime.UtcNow;

            var updated = await _clientRepository.UpdateAsync(client);
            return MapToDto(updated);
        }

        public async Task<BankClientDto> SuspendAsync(Guid id)
        {
            var client = await _clientRepository.GetByIdAsync(id);
            if (client is null)
                throw new InvalidOperationException($"Bank client with ID {id} not found.");
            if (client.Status == ClientStatus.Suspended)
                throw new InvalidOperationException($"Bank client with ID {id} is already suspended.");

            client.Status = ClientStatus.Suspended;
            client.UpdatedAt = DateTime.UtcNow;
            var updated = await _clientRepository.UpdateAsync(client);
            return MapToDto(updated);
        }

        public async Task<BankClientDto> ActivateAsync(Guid id)
        {
            var client = await _clientRepository.GetByIdAsync(id);
            if (client is null)
                throw new InvalidOperationException($"Bank client with ID {id} not found.");
            if (client.Status == ClientStatus.Active)
                throw new InvalidOperationException($"Bank client with ID {id} is already active.");

            client.Status = ClientStatus.Active;
            client.UpdatedAt = DateTime.UtcNow;
            var updated = await _clientRepository.UpdateAsync(client);
            return MapToDto(updated);
        }

        public async Task<bool> DeleteAsync(Guid id)
            => await _clientRepository.DeleteAsync(id);

        private static void ValidateName(string value, string paramName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"{paramName} cannot be empty.", paramName);
        }

        private static void ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty.", nameof(email));

            try { _ = new MailAddress(email); }
            catch (FormatException)
            {
                throw new ArgumentException($"'{email}' is not a valid email address.", nameof(email));
            }
        }

        private static BankClientDto MapToDto(BankClient client) => new BankClientDto
        {
            Id = client.Id,
            FirstName = client.FirstName,
            LastName = client.LastName,
            Email = client.Email,
            PhoneNumber = client.PhoneNumber,
            DateOfBirth = client.DateOfBirth,
            NationalId = client.NationalId,
            Street = client.Street,
            City = client.City,
            PostalCode = client.PostalCode,
            Country = client.Country,
            Status = client.Status,
            CreatedAt = client.CreatedAt,
            UpdatedAt = client.UpdatedAt
        };
    }
}
