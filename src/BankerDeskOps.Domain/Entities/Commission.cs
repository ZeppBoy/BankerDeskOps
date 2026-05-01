using BankerDeskOps.Domain.Enums;

namespace BankerDeskOps.Domain.Entities
{
    public class Commission
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string Name { get; set; } = null!;
        public decimal Percentage { get; set; }
        public CommissionType Type { get; set; } = CommissionType.Application;
    }
}
