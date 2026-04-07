using BankerDeskOps.Domain.Enums;

namespace BankerDeskOps.Application.DTOs
{
    /// <summary>
    /// DTO representing a loan for API responses.
    /// </summary>
    public class LoanDto
    {
        /// <summary>
        /// Unique identifier for the loan.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Name of the customer.
        /// </summary>
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>
        /// Loan amount.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Interest rate as percentage.
        /// </summary>
        public decimal InterestRate { get; set; }

        /// <summary>
        /// Term in months.
        /// </summary>
        public int TermMonths { get; set; }

        /// <summary>
        /// Current status of the loan.
        /// </summary>
        public LoanStatus Status { get; set; }

        /// <summary>
        /// Creation timestamp.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Last update timestamp.
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}
