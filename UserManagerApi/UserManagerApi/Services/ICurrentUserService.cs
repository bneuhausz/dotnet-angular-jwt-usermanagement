namespace UserManagerApi.Services;

public interface ICurrentUserService
{
    int? GetCurrentUserId();
    void SetCurrentUserId(int id);
}
