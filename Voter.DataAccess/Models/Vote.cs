using System.ComponentModel.DataAnnotations;

namespace Voter.DataAccess.Models
{
    public class Vote
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PollId { get; set; }
        public virtual Poll? Poll { get; set; }

        [Required]
        public int PollOptionId { get; set; }
        public virtual PollOption? PollOption { get; set; }

        [Required]
        public string UserId { get; set; } = null!;
        public virtual User? User { get; set; }

        [Required]
        public DateTime VoteDate { get; set; } = DateTime.UtcNow;
    }
}
