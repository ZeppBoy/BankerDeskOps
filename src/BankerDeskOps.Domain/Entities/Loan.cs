using BankerDeskOps.Domain.Enums;

namespace BankerDeskOps.Domain.Entities
{
    /// <summary>
    /// Represents a loan product offered to customers.
    /// </summary>
    public class Loan
    {
        /// <summary>
        /// Unique identifier for the loan.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Name of the customer who holds the loan.
        /// </summary>
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>
        /// Loan amount in currency units.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Annual interest rate expressed as a percentage.
        /// </summary>
        public decimal InterestRate { get; set; }

        /// <summary>
        /// Loan term in months.
        /// </summary>
        public int TermMonths { get; set; }

        /// <summary>
        /// Current status of the loan.
        /// </summary>
        public LoanStatus Status { get; set; } = LoanStatus.Pending;

        /// <summary>
        /// Timestamp when the loan was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Timestamp of the last update to the loan.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
