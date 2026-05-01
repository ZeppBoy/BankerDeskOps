namespace BankerDeskOps.Application.DTOs
{
    public class LoanApplicationDto
    {
        public Guid Id { get; set; }
        public string RequestId { get; set; } = null!;
        public Guid ProductId { get; set; }
        public string? ProductName { get; set; }
        public decimal Amount { get; set; }
        public int TermMonths { get; set; }
        public string Status { get; set; } = null!;
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateLoanApplicationRequest
    {
        public Guid ProductId { get; set; }
        public decimal Amount { get; set; }
        public int TermMonths { get; set; }
    }
}
