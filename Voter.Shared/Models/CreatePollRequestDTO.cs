using System.ComponentModel.DataAnnotations;

namespace Voter.Shared.Models
{
    public class CreatePollRequestDTO
    {
        [Required]
        [StringLength(200, MinimumLength = 5)]
        public string Question { get; set; } = string.Empty;
        [Required]
        public string StartDate { get; set; } = string.Empty;

        [Required]
        public string EndDate { get; set; } = string.Empty;
        [Required]
        [MinLength(2, ErrorMessage = "At least 2 options are required")]
        public List<string> Options { get; set; } = [];
    }
}
