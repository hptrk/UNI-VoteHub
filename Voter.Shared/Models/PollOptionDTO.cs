namespace Voter.Shared.Models
{
    public class PollOptionDTO
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int PollId { get; set; }
        public bool UserVoted { get; set; } = false;
    }
}
