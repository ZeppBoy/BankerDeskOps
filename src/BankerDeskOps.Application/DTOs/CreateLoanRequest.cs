using System.ComponentModel.DataAnnotations;

namespace BankerDeskOps.Application.DTOs
{
    /// <summary>
    /// Request DTO for creating a new loan.
    /// </summary>
    public class CreateLoanRequest
    {
        /// <summary>
        /// Name of the customer.
        /// </summary>
        [Required(ErrorMessage = "Customer name is required.")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Customer name must be between 1 and 200 characters.")]
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>
        /// Loan amount. Must be greater than zero.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Interest rate as percentage. Must be between 0 and 100.
        /// </summary>
        public decimal InterestRate { get; set; }

        /// <summary>
        /// Term in months. Must be positive.
        /// </summary>
        public int TermMonths { get; set; }
    }
}
