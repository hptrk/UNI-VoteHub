using Voter.Shared.Models;

namespace Voter.Blazor.WebAssembly.Services
{
    public interface IAuthService
    {
        Task<LoginResponseDTO> LoginAsync(LoginRequestDTO loginRequest);
        Task LogoutAsync();
        Task<bool> IsUserAuthenticatedAsync();
        Task<string?> GetTokenAsync();
    }
}
