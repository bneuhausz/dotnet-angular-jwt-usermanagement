using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserManagerApi.Data;
using UserManagerApi.Dtos;

namespace UserManagerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly UsersDbContext _db;

    public RolesController(UsersDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _db.Roles
            .Select(u => new RoleDto
            {
                Id = u.Id,
                Name = u.Name,
                IsDeleted = u.IsDeleted,
            })
            .AsNoTracking()
            .ToListAsync();
        return Ok(users);
    }

    [HttpGet("{roleId:guid}/permissions")]
    public async Task<IActionResult> GetPermissionsByRoleId(Guid roleId)
    {
        var userRoles = await _db.RolePermissions
            .Where(r => r.RoleId == roleId && !r.IsDeleted)
            .Select(rp => new PermissionDto
            {
                Id = rp.PermissionId,
                Name = rp.Permission.Name
            })
            .AsNoTracking()
            .ToListAsync();

        return Ok(userRoles);
    }

    [HttpPut("{roleId:guid}/togglepermission/{permissionId:guid}")]
    public async Task<IActionResult> TogglePermission(Guid roleId, Guid permissionId)
    {
        var rolePermission = await _db.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

        if (rolePermission == null)
        {
            rolePermission = new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId,
            };
            _db.RolePermissions.Add(rolePermission);
        }
        else
        {
            rolePermission.IsDeleted = !rolePermission.IsDeleted;
            _db.RolePermissions.Update(rolePermission);
        }
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
