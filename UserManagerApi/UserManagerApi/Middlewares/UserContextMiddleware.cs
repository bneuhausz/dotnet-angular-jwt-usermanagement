using UserManagerApi.Services;

namespace UserManagerApi.Middlewares;

public class UserContextMiddleware
{
    private readonly RequestDelegate _next;

    public UserContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, CurrentUserService currentUserService)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var subClaim = context.User.FindFirst("sub");
            if (subClaim != null && Guid.TryParse(subClaim.Value, out var userId))
            {
                currentUserService.UserId = userId;
            }
        }

        await _next(context);
    }
}
