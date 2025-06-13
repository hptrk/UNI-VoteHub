namespace Voter.Shared.Models
{
    public class RegisterResponseDTO
    {
        public bool Success { get; set; }
        public IEnumerable<string>? Errors { get; set; }
    }
}
