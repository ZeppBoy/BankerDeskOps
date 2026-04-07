using BankerDeskOps.Domain.Enums;

namespace BankerDeskOps.Domain.Entities
{
    /// <summary>
    /// Represents a retail bank account held by a customer.
    /// </summary>
    public class RetailAccount
    {
        /// <summary>
        /// Unique identifier for the account.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Name of the customer who holds the account.
        /// </summary>
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>
        /// Unique account number assigned to this account.
        /// </summary>
        public string AccountNumber { get; set; } = string.Empty;

        /// <summary>
        /// Current balance in the account.
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// Type of account (Checking or Savings).
        /// </summary>
        public AccountType AccountType { get; set; } = AccountType.Checking;

        /// <summary>
        /// Timestamp when the account was opened.
        /// </summary>
        public DateTime OpenedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Timestamp of the last update to the account.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
