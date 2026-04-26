namespace BankerDeskOps.Application.DTOs
{
    /// <summary>
    /// Request DTO for executing a money transfer between accounts.
    /// </summary>
    public class TransferRequest
    {
        /// <summary>
        /// Source account ID (debit).
        /// </summary>
        public Guid FromAccountId { get; set; }

        /// <summary>
        /// Destination account ID (credit).
        /// </summary>
        public Guid ToAccountId { get; set; }

        /// <summary>
        /// Amount to transfer.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Optional description of the transfer.
        /// </summary>
        public string? Description { get; set; }
    }
}