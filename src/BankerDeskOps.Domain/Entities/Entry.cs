using BankerDeskOps.Domain.Enums;

namespace BankerDeskOps.Domain.Entities
{
    /// <summary>
    /// Represents a single side of a transaction (debit or credit).
    /// </summary>
    public class Entry
    {
        /// <summary>
        /// Unique identifier for the entry.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Foreign key to the associated transaction.
        /// </summary>
        public Guid TransactionId { get; set; }

        /// <summary>
        /// Foreign key to the associated account.
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// Amount of the entry (always positive).
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Type of entry (Debit or Credit).
        /// </summary>
        public EntryType EntryType { get; set; }

        /// <summary>
        /// Account balance after this entry was applied.
        /// </summary>
        public decimal BalanceAfter { get; set; }

        /// <summary>
        /// Optional description of the entry.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Timestamp when the entry was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        /// <summary>
        /// The transaction this entry belongs to.
        /// </summary>
        public Transaction? Transaction { get; set; }

        /// <summary>
        /// The account this entry affects.
        /// </summary>
        public RetailAccount? Account { get; set; }
    }
}