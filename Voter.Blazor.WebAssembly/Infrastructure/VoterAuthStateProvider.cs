using System.Net.Http.Headers;
using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Text.Json;

namespace Voter.Blazor.WebAssembly.Infrastructure
{
    public class VoterAuthStateProvider(HttpClient httpClient, ILocalStorageService localStorage) : AuthenticationStateProvider
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ILocalStorageService _localStorage = localStorage;
        private readonly AuthenticationState _anonymous = new(new ClaimsPrincipal(new ClaimsIdentity()));

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            string? token = await _localStorage.GetItemAsStringAsync("authToken");

            if (string.IsNullOrWhiteSpace(token))
            {
                return _anonymous;
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return new AuthenticationState(
                new ClaimsPrincipal(
                    new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt")));
        }

        public void NotifyUserAuthentication(string token)
        {
            ClaimsPrincipal authenticatedUser = new(
                new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt"));

            Task<AuthenticationState> authState = Task.FromResult(new AuthenticationState(authenticatedUser));
            NotifyAuthenticationStateChanged(authState);
        }

        public void NotifyUserLogout()
        {
            Task<AuthenticationState> authState = Task.FromResult(_anonymous);
            NotifyAuthenticationStateChanged(authState);
        }

        private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            List<Claim> claims = [];
            string payload = jwt.Split('.')[1];
            byte[] jsonBytes = ParseBase64WithoutPadding(payload);
            Dictionary<string, object>? keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            if (keyValuePairs == null)
            {
                return claims;
            }

            _ = keyValuePairs.TryGetValue("role", out object? roles);

            if (roles != null)
            {
                if (roles.ToString()!.Trim().StartsWith("["))
                {
                    string[]? parsedRoles = JsonSerializer.Deserialize<string[]>(roles.ToString()!);
                    if (parsedRoles != null)
                    {
                        foreach (string parsedRole in parsedRoles)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, parsedRole));
                        }
                    }
                }
                else
                {
                    claims.Add(new Claim(ClaimTypes.Role, roles.ToString()!));
                }

                _ = keyValuePairs.Remove("role");
            }

            claims.AddRange(keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()!)));

            return claims;
        }

        private static byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
                default:
                    break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}
