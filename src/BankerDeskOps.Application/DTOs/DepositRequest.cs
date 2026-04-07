namespace BankerDeskOps.Application.DTOs
{
    /// <summary>
    /// Request DTO for depositing funds into an account.
    /// </summary>
    public class DepositRequest
    {
        /// <summary>
        /// Amount to deposit. Must be greater than zero.
        /// </summary>
        public decimal Amount { get; set; }
    }
}
