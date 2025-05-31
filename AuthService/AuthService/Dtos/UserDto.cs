namespace AuthService.Dtos;

public class UserDto
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
}
