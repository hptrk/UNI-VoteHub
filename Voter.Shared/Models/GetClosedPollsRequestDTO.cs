namespace Voter.Shared.Models
{
    public class GetClosedPollsRequestDTO
    {
        public string? QuestionFilter { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
