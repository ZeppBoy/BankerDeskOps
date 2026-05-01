namespace BankerDeskOps.Application.DTOs
{
    public class CurrencyDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
    }

    public class CreateCurrencyRequest
    {
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
    }

    public class UpdateCurrencyRequest
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
    }
}
