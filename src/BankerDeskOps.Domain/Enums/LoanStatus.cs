namespace BankerDeskOps.Domain.Enums
{
    /// <summary>
    /// Represents the status of a loan throughout its lifecycle.
    /// </summary>
    public enum LoanStatus
    {
        /// <summary>
        /// Loan application is pending approval.
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Loan has been approved.
        /// </summary>
        Approved = 1,

        /// <summary>
        /// Loan application has been rejected.
        /// </summary>
        Rejected = 2,

        /// <summary>
        /// Loan has been closed.
        /// </summary>
        Closed = 3,

        /// <summary>
        /// Loan has been disbursed — funds paid out to the customer.
        /// This is the terminal approval stage and triggers Contract creation.
        /// </summary>
        Disbursed = 4
    }
}
