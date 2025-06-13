using System.ComponentModel.DataAnnotations;

namespace Voter.DataAccess.Models
{
    public class PollOption
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public required string Text { get; set; }

        [Required]
        public int PollId { get; set; }
        public virtual Poll? Poll { get; set; }

        public virtual ICollection<Vote> Votes { get; set; } = [];
    }
}
