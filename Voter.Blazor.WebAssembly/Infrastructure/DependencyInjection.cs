using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Voter.Blazor.WebAssembly.Services;

namespace Voter.Blazor.WebAssembly.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBlazorWebAssembly(this IServiceCollection services, IConfiguration configuration)
        {
            // Add AutoMapper
            _ = services.AddAutoMapper(typeof(BlazorMappingProfile).Assembly);

            // Add HttpClient
            _ = services.AddScoped(sp => new HttpClient
            {
                BaseAddress = new Uri(configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7012")
            });

            // Configure JSON serialization
            _ = services.Configure<JsonSerializerOptions>(options =>
            {
                options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.PropertyNameCaseInsensitive = true;
                options.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });
            // Register services
            _ = services.AddScoped<IAuthService, AuthService>();
            _ = services.AddScoped<IPollService, PollService>(); // register auth state provider
            _ = services.AddScoped<AuthenticationStateProvider, VoterAuthStateProvider>();
            _ = services.AddAuthorizationCore();

            // Register local storage service
            _ = services.AddBlazoredLocalStorage();

            return services;
        }
    }
}
