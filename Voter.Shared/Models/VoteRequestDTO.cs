using System.ComponentModel.DataAnnotations;

namespace Voter.Shared.Models
{
    public class VoteRequestDTO
    {
        [Required]
        public int PollId { get; set; }

        [Required]
        public int OptionId { get; set; }
    }
}
