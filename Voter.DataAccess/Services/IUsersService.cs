using Microsoft.AspNetCore.Identity;
using Voter.DataAccess.Models;

namespace Voter.DataAccess.Services
{
    public interface IUsersService
    {
        Task<User?> GetUserByIdAsync(string id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<IdentityResult> RegisterUserAsync(User user, string password);
        Task<bool> ValidateUserAsync(string email, string password);
        Task<string> CreateTokenAsync(User user);
        Task<IEnumerable<User>> GetAllUsersAsync();
    }
}
