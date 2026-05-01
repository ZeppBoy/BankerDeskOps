namespace BankerDeskOps.Application.DTOs
{
    public class RateDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string? ProductName { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public int MinTermMonths { get; set; }
        public int MaxTermMonths { get; set; }
        public decimal RateValue { get; set; }
    }

    public class CreateRateRequest
    {
        public Guid ProductId { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public int MinTermMonths { get; set; }
        public int MaxTermMonths { get; set; }
        public decimal RateValue { get; set; }
    }

    public class UpdateRateRequest
    {
        public Guid Id { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public int MinTermMonths { get; set; }
        public int MaxTermMonths { get; set; }
        public decimal RateValue { get; set; }
    }
}
