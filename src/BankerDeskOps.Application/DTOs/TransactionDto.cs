namespace BankerDeskOps.Application.DTOs
{
    /// <summary>
    /// DTO representing a transaction for API responses.
    /// </summary>
    public class TransactionDto
    {
        /// <summary>
        /// Unique identifier for the transaction.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Type of transaction.
        /// </summary>
        public Domain.Enums.TransactionType TransactionType { get; set; }

        /// <summary>
        /// Current status of the transaction.
        /// </summary>
        public Domain.Enums.TransactionStatus Status { get; set; }

        /// <summary>
        /// Unique reference ID for external systems.
        /// </summary>
        public string? ReferenceId { get; set; }

        /// <summary>
        /// Timestamp when the transaction was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Collection of entries associated with this transaction.
        /// </summary>
        public ICollection<EntryDto> Entries { get; set; } = new List<EntryDto>();
    }
}