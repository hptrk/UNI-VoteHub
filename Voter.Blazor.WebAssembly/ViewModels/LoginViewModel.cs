using System.ComponentModel.DataAnnotations;
using Voter.Shared.Models;

namespace Voter.Blazor.WebAssembly.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public LoginRequestDTO ToLoginRequestDTO()
        {
            return new LoginRequestDTO
            {
                Email = Email,
                Password = Password
            };
        }
    }
}
