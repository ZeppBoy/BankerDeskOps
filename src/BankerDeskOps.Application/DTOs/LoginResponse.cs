namespace BankerDeskOps.Application.DTOs
{
    public class LoginResponse
    {
        public bool Success { get; set; }
        public UserDto? User { get; set; }
        public bool IsAnonymous { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
