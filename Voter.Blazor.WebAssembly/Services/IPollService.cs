using Voter.Blazor.WebAssembly.ViewModels;
using Voter.Shared.Models;

namespace Voter.Blazor.WebAssembly.Services
{
    public interface IPollService
    {
        Task<List<PollDTO>> GetActivePollsAsync();
        Task<List<PollDTO>> GetClosedPollsAsync();
        Task<PollDTO> GetPollByIdAsync(int id);
        Task<PollResultDTO> GetPollResultsAsync(int pollId);
        Task<PollDTO> CreatePollAsync(CreatePollViewModel createPollViewModel);
        Task<List<UserDTO>> GetPollVotersAsync(int pollId);
        Task<List<PollDTO>> GetUserPollsAsync();
    }
}
