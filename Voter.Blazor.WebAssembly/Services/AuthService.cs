using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Voter.Blazor.WebAssembly.Infrastructure;
using Voter.Shared.Models;

namespace Voter.Blazor.WebAssembly.Services
{
    public class AuthService(
        HttpClient httpClient,
        ILocalStorageService localStorage,
        AuthenticationStateProvider authStateProvider) : IAuthService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ILocalStorageService _localStorage = localStorage;
        private readonly AuthenticationStateProvider _authStateProvider = authStateProvider;
        private const string TokenKey = "authToken"; public async Task<LoginResponseDTO> LoginAsync(LoginRequestDTO loginRequest)
        {
            LoginResponseDTO response = await HttpRequestUtility.SendRequestAsync<LoginResponseDTO>(
                _httpClient, HttpMethod.Post, "api/auth/login", loginRequest);

            if (response.Success && !string.IsNullOrEmpty(response.Token))
            {
                await _localStorage.SetItemAsStringAsync(TokenKey, response.Token);
                ((VoterAuthStateProvider)_authStateProvider).NotifyUserAuthentication(response.Token);
            }

            return response;
        }
        public async Task LogoutAsync()
        {
            await _localStorage.RemoveItemAsync(TokenKey);
            ((VoterAuthStateProvider)_authStateProvider).NotifyUserLogout();
        }
        public async Task<bool> IsUserAuthenticatedAsync()
        {
            string? token = await _localStorage.GetItemAsStringAsync(TokenKey);
            return !string.IsNullOrEmpty(token);
        }

        public async Task<string?> GetTokenAsync()
        {
            return await _localStorage.GetItemAsStringAsync(TokenKey);
        }
    }
}
