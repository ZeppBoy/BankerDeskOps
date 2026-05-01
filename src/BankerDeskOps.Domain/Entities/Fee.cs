using BankerDeskOps.Domain.Enums;

namespace BankerDeskOps.Domain.Entities
{
    public class Fee
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string Name { get; set; } = null!;
        public decimal Amount { get; set; }
        public FeeType Type { get; set; } = FeeType.Disbursement;
    }
}
