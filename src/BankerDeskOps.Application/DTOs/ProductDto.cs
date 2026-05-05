namespace BankerDeskOps.Application.DTOs
{
    public class ProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int Term { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public Guid CurrencyId { get; set; }
    }

    public class CreateProductRequest
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public int Term { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public Guid CurrencyId { get; set; }
    }

    public class UpdateProductRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int Term { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public Guid CurrencyId { get; set; }
    }
}
