using System.ComponentModel.DataAnnotations;
using BankerDeskOps.Domain.Enums;

namespace BankerDeskOps.Application.DTOs
{
    public class UpdateUserRequest
    {
        [Required]
        [StringLength(255, MinimumLength = 1)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string LastName { get; set; } = string.Empty;

        public UserRole Role { get; set; } = UserRole.Operator;
    }
}
