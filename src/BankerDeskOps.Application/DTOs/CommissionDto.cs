namespace BankerDeskOps.Application.DTOs
{
    public class CommissionDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string? ProductName { get; set; }
        public string Name { get; set; } = null!;
        public decimal Percentage { get; set; }
    }

    public class CreateCommissionRequest
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; } = null!;
        public decimal Percentage { get; set; }
    }

    public class UpdateCommissionRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Percentage { get; set; }
    }
}
