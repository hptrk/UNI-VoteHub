namespace Voter.Shared.Models
{
    public class PollResultDTO
    {
        public int PollId { get; set; }
        public string Question { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string CreatorUsername { get; set; } = string.Empty;
        public string CreatorEmail { get; set; } = string.Empty;
        public List<OptionResultDTO> Results { get; set; } = [];
        public int TotalVotes { get; set; }
        public int? UserVoteOptionId { get; set; }
    }

    public class OptionResultDTO
    {
        public int OptionId { get; set; }
        public string OptionText { get; set; } = string.Empty;
        public int VoteCount { get; set; }
        public double Percentage { get; set; }
    }
}
