namespace BankerDeskOps.Application.DTOs
{
    /// <summary>
    /// Request DTO for withdrawing funds from an account.
    /// </summary>
    public class WithdrawRequest
    {
        /// <summary>
        /// Amount to withdraw. Must be greater than zero.
        /// </summary>
        public decimal Amount { get; set; }
    }
}
