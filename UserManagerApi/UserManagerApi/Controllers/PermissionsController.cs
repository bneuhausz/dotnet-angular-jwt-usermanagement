using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserManagerApi.Attributes;
using UserManagerApi.Data;
using UserManagerApi.Dtos;

namespace UserManagerApi.Controllers;

[CheckPermissions("Roles")]
public class PermissionsController : MaintenanceController
{
    private readonly UsersDbContext _db;

    public PermissionsController(UsersDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var permissions = await _db.Permissions
            .Select(p => new PermissionDto
            {
                Id = p.Id,
                Name = p.Name,
                Type = p.Type,
                ParentPermissionId = p.ParentPermissionId
            })
            .AsNoTracking()
            .ToListAsync();

        return Ok(permissions);
    }
}
