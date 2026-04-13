using System.ComponentModel.DataAnnotations;

namespace BankerDeskOps.Application.DTOs
{
    public class CreateBankClientRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [StringLength(255, MinimumLength = 1)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string NationalId { get; set; } = string.Empty;

        [StringLength(255)]
        public string Street { get; set; } = string.Empty;

        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [StringLength(20)]
        public string PostalCode { get; set; } = string.Empty;

        [StringLength(100)]
        public string Country { get; set; } = string.Empty;
    }
}
