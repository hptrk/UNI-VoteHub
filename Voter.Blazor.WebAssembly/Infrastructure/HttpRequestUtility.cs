using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using Voter.Blazor.WebAssembly.Exceptions;
using Voter.Shared.Models;

namespace Voter.Blazor.WebAssembly.Infrastructure
{
    public static class HttpRequestUtility
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        }; public static async Task<T> SendRequestAsync<T>(HttpClient httpClient, HttpMethod method, string uri, object? content = null, string? token = null)
        {
            HttpRequestMessage request = new(method, uri);

            // add token if provided
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // add content if provided
            if (content != null)
            {
                string json = JsonSerializer.Serialize(content, _jsonOptions);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            // send request
            HttpResponseMessage response = await httpClient.SendAsync(request);

            // handle non-success status codes
            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                ProblemDetails? problemDetails = null;

                try
                {
                    problemDetails = JsonSerializer.Deserialize<ProblemDetails>(errorContent, _jsonOptions);
                }
                catch
                {
                    // swallow deserilaziation errors
                }
                string? errorMessage = problemDetails?.Detail ?? problemDetails?.Title ?? response.ReasonPhrase;
                throw new HttpRequestErrorException((int)response.StatusCode, errorMessage ?? "Unknown error occurred");
            }

            // return deserialized response
            return await response.Content.ReadFromJsonAsync<T>(_jsonOptions)
                ?? throw new JsonException("Failed to deserialize the response");
        }
        public static async Task SendRequestAsync(HttpClient httpClient, HttpMethod method, string uri, object? content = null, string? token = null)
        {
            HttpRequestMessage request = new(method, uri);

            // add token if provided
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // add content if provided
            if (content != null)
            {
                string json = JsonSerializer.Serialize(content, _jsonOptions);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            // send request
            HttpResponseMessage response = await httpClient.SendAsync(request);

            // handle non-success status codes
            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                ProblemDetails? problemDetails = null;

                try
                {
                    problemDetails = JsonSerializer.Deserialize<ProblemDetails>(errorContent, _jsonOptions);
                }
                catch
                {
                    // swallow deserilaziation errors
                }

                string? errorMessage = problemDetails?.Detail ?? problemDetails?.Title ?? response.ReasonPhrase;
                throw new HttpRequestErrorException((int)response.StatusCode, errorMessage ?? "Unknown error occurred");
            }
        }
    }
}
