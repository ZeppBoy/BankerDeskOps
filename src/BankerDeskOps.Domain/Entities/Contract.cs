using BankerDeskOps.Domain.Enums;

namespace BankerDeskOps.Domain.Entities
{
    /// <summary>
    /// Represents a legally binding loan contract created upon disbursement.
    /// Created automatically when a loan transitions to <see cref="LoanStatus.Disbursed"/>.
    /// </summary>
    public class Contract
    {
        /// <summary>Unique identifier for the contract.</summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Human-readable contract number, e.g. CNT-2026-A3B4C5D6.
        /// Generated once at creation and never changes.
        /// </summary>
        public string ContractNumber { get; set; } = string.Empty;

        /// <summary>
        /// Reference to the originating loan.
        /// Stored as a plain Guid — no EF navigation property by design,
        /// consistent with the rest of the domain model.
        /// </summary>
        public Guid LoanId { get; set; }

        /// <summary>Customer name copied from the loan at disbursement time.</summary>
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>Disbursed loan amount.</summary>
        public decimal LoanAmount { get; set; }

        /// <summary>Annual interest rate expressed as a percentage.</summary>
        public decimal InterestRate { get; set; }

        /// <summary>Loan term in months.</summary>
        public int TermMonths { get; set; }

        /// <summary>UTC timestamp when funds were disbursed.</summary>
        public DateTime DisbursedAt { get; set; }

        /// <summary>Current contract status.</summary>
        public ContractStatus Status { get; set; } = ContractStatus.Active;

        /// <summary>Record creation timestamp (UTC).</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Last update timestamp (UTC).</summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
