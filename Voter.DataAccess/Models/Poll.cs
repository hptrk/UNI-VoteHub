using System.ComponentModel.DataAnnotations;

namespace Voter.DataAccess.Models
{
    public class Poll
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public required string Question { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public string CreatorId { get; set; } = null!;
        public virtual User? Creator { get; set; }

        public virtual ICollection<PollOption> Options { get; set; } = [];
        public virtual ICollection<Vote> Votes { get; set; } = [];
    }
}
