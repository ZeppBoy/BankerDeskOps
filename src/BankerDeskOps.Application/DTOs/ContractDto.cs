using BankerDeskOps.Domain.Enums;

namespace BankerDeskOps.Application.DTOs
{
    /// <summary>
    /// DTO representing a loan contract for API responses.
    /// </summary>
    public class ContractDto
    {
        public Guid Id { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public Guid LoanId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal LoanAmount { get; set; }
        public decimal InterestRate { get; set; }
        public int TermMonths { get; set; }
        public DateTime DisbursedAt { get; set; }
        public ContractStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
