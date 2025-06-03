namespace UserManagerApi.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string UserIdKey = "CurrentUserId";

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? GetCurrentUserId()
    {
        return _httpContextAccessor.HttpContext?.Items[UserIdKey] as int?;
    }

    public void SetCurrentUserId(int id)
    {
        _httpContextAccessor.HttpContext!.Items[UserIdKey] = id;
    }
}
