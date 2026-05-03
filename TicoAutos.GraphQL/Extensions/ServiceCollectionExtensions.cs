using HotChocolate.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TicoAutos.GraphQL.Queries;

namespace TicoAutos.GraphQL.Extensions;

/// <summary>
/// Extension methods for registering GraphQL services and JWT authentication.
/// Keeps Program.cs clean following Single Responsibility Principle.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers JWT authentication using the same configuration as TicoAutos.WebApi,
    /// ensuring the same token works across both services.
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["Secret"]
            ?? throw new InvalidOperationException("JWT Secret is missing!");
        var key = Encoding.UTF8.GetBytes(secretKey);

        services.AddAuthentication(opt =>
        {
            opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(opt =>
        {
            opt.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };

            // Allow JWT via WebSocket for GraphQL subscriptions
            opt.Events = new JwtBearerEvents
            {
                OnMessageReceived = ctx =>
                {
                    var token = ctx.Request.Query["access_token"];
                    if (!string.IsNullOrEmpty(token))
                        ctx.Token = token;
                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization();
        return services;
    }

    /// <summary>
    /// Registers the HotChocolate GraphQL server with all query types and JWT authorization.
    /// </summary>
    public static IServiceCollection AddGraphQlServer(
        this IServiceCollection services)
    {
        services
            .AddGraphQLServer()
            .AddQueryType()
            .AddTypeExtension<VehicleQuery>()
            .AddTypeExtension<QuestionQuery>()
            .AddAuthorization()
            .AddHttpRequestInterceptor<HttpRequestInterceptor>();

        return services;
    }
}