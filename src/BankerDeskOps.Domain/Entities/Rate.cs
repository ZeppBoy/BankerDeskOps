using BankerDeskOps.Domain.Enums;

namespace BankerDeskOps.Domain.Entities
{
    public class Rate
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public int MinTermMonths { get; set; }
        public int MaxTermMonths { get; set; }
        public decimal RateValue { get; set; }
        public DateTime SinceDate { get; set; }
        public DateTime? ToDate { get; set; }
        public RateType RateType { get; set; } = RateType.Fixed;
    }
}
