namespace UserManagerApi.Dtos;

public class UserDto
{
    public required int Id { get; set; }
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public bool? IsDeleted { get; set; }
}
