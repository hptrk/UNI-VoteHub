using Voter.Blazor.WebAssembly.Infrastructure;
using Voter.Blazor.WebAssembly.ViewModels;
using Voter.Shared.Models;

namespace Voter.Blazor.WebAssembly.Services
{
    public class PollService(
        HttpClient httpClient,
        IAuthService authService) : IPollService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly IAuthService _authService = authService;
        public async Task<List<PollDTO>> GetActivePollsAsync()
        {
            string? token = await _authService.GetTokenAsync();
            return await HttpRequestUtility.SendRequestAsync<List<PollDTO>>(
                _httpClient, HttpMethod.Get, "api/polls/active", token: token);
        }
        public async Task<List<PollDTO>> GetClosedPollsAsync()
        {
            string? token = await _authService.GetTokenAsync();
            return await HttpRequestUtility.SendRequestAsync<List<PollDTO>>(
                _httpClient, HttpMethod.Get, "api/polls/closed", token: token);
        }
        public async Task<PollDTO> GetPollByIdAsync(int id)
        {
            string? token = await _authService.GetTokenAsync();
            return await HttpRequestUtility.SendRequestAsync<PollDTO>(
                _httpClient, HttpMethod.Get, $"api/polls/{id}", token: token);
        }
        public async Task<PollDTO> CreatePollAsync(CreatePollViewModel viewModel)
        {
            string? token = await _authService.GetTokenAsync();

            // Map the view model to DTO
            CreatePollRequestDTO createPollRequest = new()
            {
                Question = viewModel.Question,
                StartDate = viewModel.StartDate.ToString("yyyy-MM-dd"),
                EndDate = viewModel.EndDate.ToString("yyyy-MM-dd"),
                Options = [.. viewModel.Options.Where(o => !string.IsNullOrWhiteSpace(o))]
            };

            return await HttpRequestUtility.SendRequestAsync<PollDTO>(
                _httpClient, HttpMethod.Post, "api/polls", createPollRequest, token);
        }
        public async Task<PollResultDTO> GetPollResultsAsync(int pollId)
        {
            string? token = await _authService.GetTokenAsync();
            return await HttpRequestUtility.SendRequestAsync<PollResultDTO>(
                _httpClient, HttpMethod.Get, $"api/polls/{pollId}/results", token: token);
        }
        public async Task<List<UserDTO>> GetPollVotersAsync(int pollId)
        {
            string? token = await _authService.GetTokenAsync();
            return await HttpRequestUtility.SendRequestAsync<List<UserDTO>>(
                _httpClient, HttpMethod.Get, $"api/polls/{pollId}/voters", token: token);
        }
        public async Task<List<PollDTO>> GetUserPollsAsync()
        {
            string? token = await _authService.GetTokenAsync();
            return await HttpRequestUtility.SendRequestAsync<List<PollDTO>>(
                _httpClient, HttpMethod.Get, "api/polls/user", token: token);
        }
    }
}
