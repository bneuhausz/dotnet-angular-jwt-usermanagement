using UserManagerApi.Data;

namespace UserManagerApi.Dtos;

public class PermissionDto
{
    public required int Id { get; set; }
    public int? ParentPermissionId { get; set; }
    public required string Name { get; set; }
    public PermissionType? Type { get; set; }
}
