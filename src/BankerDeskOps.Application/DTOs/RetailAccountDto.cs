using BankerDeskOps.Domain.Enums;

namespace BankerDeskOps.Application.DTOs
{
    /// <summary>
    /// DTO representing a retail account for API responses.
    /// </summary>
    public class RetailAccountDto
    {
        /// <summary>
        /// Unique identifier for the account.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Name of the customer.
        /// </summary>
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>
        /// Account number.
        /// </summary>
        public string AccountNumber { get; set; } = string.Empty;

        /// <summary>
        /// IBAN code for the account.
        /// </summary>
        public string Iban { get; set; } = string.Empty;

        /// <summary>
        /// Current balance.
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// Type of account.
        /// </summary>
        public AccountType AccountType { get; set; }

        /// <summary>
        /// Account opening timestamp.
        /// </summary>
        public DateTime OpenedAt { get; set; }

        /// <summary>
        /// Last update timestamp.
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}
