using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Voter.DataAccess.Config;
using Voter.DataAccess.Models;

namespace Voter.DataAccess.Services
{
    public class UsersService(
        UserManager<User> userManager,
        IOptions<JwtSettings> jwtSettings,
        VoterDbContext dbContext) : IUsersService
    {
        private readonly UserManager<User> _userManager = userManager;
        private readonly JwtSettings _jwtSettings = jwtSettings.Value;
        private readonly VoterDbContext _dbContext = dbContext;

        public async Task<User?> GetUserByIdAsync(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }
        public async Task<IdentityResult> RegisterUserAsync(User user, string password)
        {
            IdentityResult result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                _ = await _userManager.AddToRoleAsync(user, "User");
            }
            return result;
        }
        public async Task<bool> ValidateUserAsync(string email, string password)
        {
            User? user = await _userManager.FindByEmailAsync(email);
            return user != null && await _userManager.CheckPasswordAsync(user, password);
        }
        public async Task<string> CreateTokenAsync(User user)
        {
            SigningCredentials signingCredentials = GetSigningCredentials();
            List<Claim> claims = await GetClaimsAsync(user);
            JwtSecurityToken tokenOptions = GenerateTokenOptions(signingCredentials, claims);

            string token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
            return token;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _dbContext.Users.ToListAsync();
        }
        private SigningCredentials GetSigningCredentials()
        {
            byte[] key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);
            SymmetricSecurityKey secret = new(key);

            return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
        }
        private async Task<List<Claim>> GetClaimsAsync(User user)
        {
            List<Claim> claims =
            [
                new(ClaimTypes.Name, user.Email!),
                new(ClaimTypes.Email, user.Email!),
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            ];

            IList<string> roles = await _userManager.GetRolesAsync(user);
            foreach (string role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            return claims;
        }
        private JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims)
        {
            JwtSecurityToken tokenOptions = new(
                issuer: _jwtSettings.ValidIssuer,
                audience: _jwtSettings.ValidAudience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_jwtSettings.ExpiryMinutes),
                signingCredentials: signingCredentials
            );

            return tokenOptions;
        }
    }
}
