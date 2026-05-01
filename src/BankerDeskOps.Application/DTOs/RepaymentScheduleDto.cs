namespace BankerDeskOps.Application.DTOs
{
    public class RepaymentScheduleDto
    {
        public Guid ScheduleId { get; set; }
        public Guid LoanApplicationId { get; set; }
        public int PaymentNumber { get; set; }
        public DateTime DueDate { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal TotalPayment { get; set; }
    }

    public class CreateRepaymentScheduleRequest
    {
        public Guid LoanApplicationId { get; set; }
        public int PaymentNumber { get; set; }
        public DateTime DueDate { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal TotalPayment { get; set; }
    }
}
