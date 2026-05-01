namespace BankerDeskOps.Domain.Entities
{
    public class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public int Term { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public Guid CurrencyId { get; set; }
        public Currency? Currency { get; set; }
        public string? Fees { get; set; }
        public string? Commissions { get; set; }
        public ICollection<LoanApplication> LoanApplications { get; set; } = new List<LoanApplication>();
    }
}
