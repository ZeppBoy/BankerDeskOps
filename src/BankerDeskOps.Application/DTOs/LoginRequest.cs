using System.ComponentModel.DataAnnotations;

namespace BankerDeskOps.Application.DTOs
{
    public class LoginRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}
