namespace UserManagerApi.Dtos;

public class RoleDto
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public bool? IsDeleted { get; set; }
}
