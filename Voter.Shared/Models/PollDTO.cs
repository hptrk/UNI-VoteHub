
namespace Voter.Shared.Models
{
    public class PollDTO
    {
        public int Id { get; set; }
        public string Question { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string CreatorId { get; set; } = string.Empty;
        public string CreatorEmail { get; set; } = string.Empty;
        public List<PollOptionDTO> Options { get; set; } = [];
        public bool UserHasVoted { get; set; }
    }
}
