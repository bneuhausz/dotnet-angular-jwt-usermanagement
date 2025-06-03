using UserManagerApi.Services;

namespace UserManagerApi.Middlewares;

public class UserContextMiddleware
{
    private readonly RequestDelegate _next;

    public UserContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICurrentUserService currentUserService)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var subClaim = context.User.FindFirst("sub");
            if (subClaim != null && int.TryParse(subClaim.Value, out var userId))
            {
                currentUserService.SetCurrentUserId(userId);
            }
        }

        await _next(context);
    }
}
