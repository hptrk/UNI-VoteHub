using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Voter.DataAccess.Config;
using Voter.DataAccess.Models;
using Voter.DataAccess.Services;

namespace Voter.DataAccess
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration)
        {
            // configure DbContext
            _ = services.AddDbContext<VoterDbContext>(options =>
                options.UseSqlite(configuration.GetConnectionString("VoterConnection"))
                .UseLazyLoadingProxies());

            // configure Identity
            _ = services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<VoterDbContext>()
                .AddDefaultTokenProviders();

            // configure Identity Options
            _ = services.Configure<IdentityOptions>(options =>
            {
                // password settings
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 6;

                // user settings
                options.User.RequireUniqueEmail = true;
            });

            // JWT
            JwtSettings jwtSettings = new();
            configuration.GetSection("JwtSettings").Bind(jwtSettings);
            _ = services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

            _ = services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.ValidIssuer,
                    ValidAudience = jwtSettings.ValidAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.Secret))
                };
            });

            // services
            _ = services.AddScoped<IPollsService, PollsService>();
            _ = services.AddScoped<IUsersService, UsersService>();

            return services;
        }
    }
}
