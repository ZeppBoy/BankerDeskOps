using System.ComponentModel.DataAnnotations;
using BankerDeskOps.Domain.Enums;

namespace BankerDeskOps.Application.DTOs
{
    /// <summary>
    /// Request DTO for creating a new retail account.
    /// </summary>
    public class CreateRetailAccountRequest
    {
        /// <summary>
        /// Name of the customer.
        /// </summary>
        [Required(ErrorMessage = "Customer name is required.")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Customer name must be between 1 and 200 characters.")]
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>
        /// Type of account to open.
        /// </summary>
        [Required(ErrorMessage = "Account type is required.")]
        public AccountType AccountType { get; set; } = AccountType.Checking;

        /// <summary>
        /// Initial deposit amount. Cannot be negative.
        /// </summary>
        public decimal InitialDeposit { get; set; }
    }
}
