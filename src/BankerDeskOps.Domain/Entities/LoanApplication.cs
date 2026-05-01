using BankerDeskOps.Domain.Enums;

namespace BankerDeskOps.Domain.Entities
{
    public class LoanApplication
    {
        public Guid Id { get; set; }
        public string RequestId { get; set; } = null!;
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }
        public decimal Amount { get; set; }
        public int TermMonths { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? Comment { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime PendingDate { get; set; }
        public DateTime DisbursementDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public DateTime? RejectedDate { get; set; }
        public LoanApplicationStatus Status { get; set; } = LoanApplicationStatus.Pending;
        public decimal TotalAmount { get; set; }
        public string? RepaymentPlan { get; set; }
        public ICollection<RepaymentSchedule> RepaymentSchedules { get; set; } = new List<RepaymentSchedule>();
    }

    public enum LoanApplicationStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        UnderReview = 3
    }
}
