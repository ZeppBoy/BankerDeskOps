namespace BankerDeskOps.Application.DTOs
{
    /// <summary>
    /// DTO representing an entry for API responses.
    /// </summary>
    public class EntryDto
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
        public Domain.Enums.EntryType EntryType { get; set; }

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
        public DateTime CreatedAt { get; set; }
    }
}