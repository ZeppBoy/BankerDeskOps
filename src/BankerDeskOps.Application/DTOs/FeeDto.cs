namespace BankerDeskOps.Application.DTOs
{
    public class FeeDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string? ProductName { get; set; }
        public string Name { get; set; } = null!;
        public decimal Amount { get; set; }
    }

    public class CreateFeeRequest
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; } = null!;
        public decimal Amount { get; set; }
    }

    public class UpdateFeeRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Amount { get; set; }
    }
}
