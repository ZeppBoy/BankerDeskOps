using BankerDeskOps.Domain.Enums;

namespace BankerDeskOps.Domain.Entities
{
    /// <summary>
    /// Represents a money transfer between accounts.
    /// </summary>
    public class Transaction
    {
        /// <summary>
        /// Unique identifier for the transaction.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Type of transaction (Transfer, Deposit, Withdrawal).
        /// </summary>
        public TransactionType TransactionType { get; set; } = TransactionType.Transfer;

        /// <summary>
        /// Current status of the transaction.
        /// </summary>
        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;

        /// <summary>
        /// Unique reference ID for external systems.
        /// </summary>
        public string? ReferenceId { get; set; }

        /// <summary>
        /// Timestamp when the transaction was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        /// <summary>
        /// Collection of entries associated with this transaction.
        /// </summary>
        public ICollection<Entry> Entries { get; set; } = new List<Entry>();
    }
}