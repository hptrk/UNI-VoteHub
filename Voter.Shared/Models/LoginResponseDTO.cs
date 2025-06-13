namespace Voter.Shared.Models
{
    public class LoginResponseDTO
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public string? Message { get; set; }
        public UserDTO? User { get; set; }
    }
}
