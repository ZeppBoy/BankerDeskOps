namespace BankerDeskOps.Application.DTOs
{
    public class LoanCalculationRequest
    {
        public Guid ProductId { get; set; }
        public decimal Amount { get; set; }
        public int TermMonths { get; set; }
    }

    public class LoanCalculationResultDto
    {
        public decimal MonthlyPayment { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalInterest { get; set; }
        public decimal InterestRate { get; set; }
        public List<RepaymentScheduleDto> Schedule { get; set; } = new();
    }
}
