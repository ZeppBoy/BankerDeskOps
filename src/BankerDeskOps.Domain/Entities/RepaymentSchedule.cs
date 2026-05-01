namespace BankerDeskOps.Domain.Entities
{
    public class RepaymentSchedule
    {
        public Guid ScheduleId { get; set; }
        public Guid LoanApplicationId { get; set; }
        public int PaymentNumber { get; set; }
        public DateTime DueDate { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal TotalPayment { get; set; }
        public DateTime PlannedDate { get; set; }
        public decimal Capital { get; set; }
        public decimal Interest { get; set; }
        public decimal Saldo { get; set; }
        public decimal EIR { get; set; }
        public LoanApplication? LoanApplication { get; set; }
    }
}
