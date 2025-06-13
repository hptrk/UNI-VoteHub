using Voter.DataAccess.Models;

namespace Voter.DataAccess.Services
{
    public interface IPollsService
    {
        // Poll CRUD operations
        Task<IEnumerable<Poll>> GetAllPollsAsync();
        Task<IEnumerable<Poll>> GetActivePolls(string? questionFilter = null);
        Task<IEnumerable<Poll>> GetClosedPolls(string? questionFilter = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<Poll>> GetUserPolls(string userId);
        Task<Poll> GetPollByIdAsync(int id);
        Task<Poll> CreatePollAsync(Poll poll);
        // Vote operations
        Task<bool> HasUserVotedAsync(int pollId, string userId);
        Task<Vote> VoteAsync(int pollId, int optionId, string userId);
        Task<Vote?> GetUserVoteAsync(int pollId, string userId);
        Task<IEnumerable<User>> GetVotersAsync(int pollId);
        Task<Dictionary<PollOption, int>> GetPollResultsAsync(int pollId);
    }
}
