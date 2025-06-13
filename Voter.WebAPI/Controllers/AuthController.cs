using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Voter.DataAccess.Models;
using Voter.DataAccess.Services;
using Voter.Shared.Models;

namespace Voter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(
        IUsersService usersService,
        IMapper mapper,
        UserManager<User> userManager) : ControllerBase
    {
        private readonly IUsersService _usersService = usersService;
        private readonly IMapper _mapper = mapper;
        private readonly UserManager<User> _userManager = userManager;

        [HttpPost("register")]
        [ProducesResponseType(typeof(RegisterResponseDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(RegisterResponseDTO), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new RegisterResponseDTO
                {
                    Success = false,
                    Errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                });
            }
            User user = new()
            {
                UserName = request.Username,
                Email = request.Email,
                EmailConfirmed = true // auto-confirm email!!
            };

            IdentityResult result = await _usersService.RegisterUserAsync(user, request.Password);

            return !result.Succeeded
                ? BadRequest(new RegisterResponseDTO
                {
                    Success = false,
                    Errors = result.Errors.Select(e => e.Description)
                })
                : Created(string.Empty, new RegisterResponseDTO { Success = true });
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(LoginResponseDTO), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(LoginResponseDTO), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new LoginResponseDTO
                {
                    Success = false,
                    Message = "Invalid request"
                });
            }

            bool isValid = await _usersService.ValidateUserAsync(request.Email, request.Password);
            if (!isValid)
            {
                return Unauthorized(new LoginResponseDTO
                {
                    Success = false,
                    Message = "Invalid email or password"
                });
            }

            User? user = await _usersService.GetUserByEmailAsync(request.Email);
            if (user == null)
            {
                return Unauthorized(new LoginResponseDTO
                {
                    Success = false,
                    Message = "User not found"
                });
            }

            UserDTO userDTO = _mapper.Map<UserDTO>(user);
            userDTO.Roles = [.. await _userManager.GetRolesAsync(user)];

            string token = await _usersService.CreateTokenAsync(user);

            return Ok(new LoginResponseDTO
            {
                Success = true,
                Token = token,
                User = userDTO
            });
        }
    }
}
