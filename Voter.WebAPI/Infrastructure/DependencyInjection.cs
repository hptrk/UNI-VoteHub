using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

namespace Voter.WebAPI.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddWebApi(this IServiceCollection services)
        {
            _ = services.AddExceptionHandler<ExceptionToProblemDetailsHandler>();

            _ = services.AddAutoMapper(typeof(MappingProfile).Assembly);

            _ = services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                });

            // CORS
            _ = services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    _ = builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            // Swagger
            _ = services.AddEndpointsApiExplorer();
            _ = services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Voter API", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            return services;
        }
    }
}
