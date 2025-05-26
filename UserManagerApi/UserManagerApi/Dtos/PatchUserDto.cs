namespace UserManagerApi.Dtos;

public class PatchUserDto
{
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
}
