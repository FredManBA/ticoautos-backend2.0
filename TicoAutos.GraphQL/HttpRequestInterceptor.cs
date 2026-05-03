using HotChocolate.AspNetCore;
using HotChocolate.Execution;
using System.Security.Claims;

namespace TicoAutos.GraphQL;

/// <summary>
/// Intercepts every GraphQL HTTP request to extract the authenticated user's ID
/// from the JWT claims and inject it as a GlobalState value.
/// This allows query resolvers to access the current user without coupling
/// to the HTTP context directly, following Clean Architecture principles.
/// </summary>
public class HttpRequestInterceptor : DefaultHttpRequestInterceptor
{
    public override ValueTask OnCreateAsync(
        HttpContext context,
        IRequestExecutor requestExecutor,
        IQueryRequestBuilder requestBuilder,
        CancellationToken cancellationToken)
    {
        var userIdClaim = context.User.FindFirst("id")?.Value;

        if (int.TryParse(userIdClaim, out var userId))
            requestBuilder.SetGlobalState("userId", userId);

        return base.OnCreateAsync(
            context,
            requestExecutor,
            requestBuilder,
            cancellationToken);
    }
}