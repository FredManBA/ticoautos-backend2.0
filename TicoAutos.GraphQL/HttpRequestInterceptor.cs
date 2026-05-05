using HotChocolate.AspNetCore;
using HotChocolate.Execution;

namespace TicoAutos.GraphQL;

public class HttpRequestInterceptor : DefaultHttpRequestInterceptor
{
    public override ValueTask OnCreateAsync(
        HttpContext context,
        IRequestExecutor requestExecutor,
        OperationRequestBuilder requestBuilder,
        CancellationToken cancellationToken)
    {
        var userIdClaim = context.User.FindFirst("id")?.Value;
        var userId = int.TryParse(userIdClaim, out var parsed) ? parsed : 0;
        requestBuilder.SetGlobalState("userId", userId);

        return base.OnCreateAsync(context, requestExecutor, requestBuilder, cancellationToken);
    }
}
