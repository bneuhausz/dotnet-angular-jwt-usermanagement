namespace UserManagerApi.Dtos;

public class PermissionDto
{
    public required Guid Id { get; set; }
    public Guid? ParentPermissionId { get; set; }
    public required string Name { get; set; }
    public string? Type { get; set; }
}
