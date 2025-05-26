namespace UserManagerApi.Dtos;

public class RoleDto
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public bool? IsDeleted { get; set; }
}
