using HotChocolate.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TicoAutos.GraphQL.Queries;

namespace TicoAutos.GraphQL.Extensions;

public static class ServiceCollectionExtensions
{
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

    public static IServiceCollection AddGraphQlServer(
        this IServiceCollection services)
    {
        services.AddScoped<VehicleQuery>();
        services.AddScoped<QuestionQuery>();

        services
            .AddGraphQLServer()
            .AddQueryType()
            .AddTypeExtension<VehicleQuery>()
            .AddTypeExtension<QuestionQuery>()
            .AddHttpRequestInterceptor<HttpRequestInterceptor>()
            .ModifyRequestOptions(o => o.IncludeExceptionDetails = true);

        return services;
    }
}
