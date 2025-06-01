using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserManagerApi.Attributes;
using UserManagerApi.Data;
using UserManagerApi.Dtos;

namespace UserManagerApi.Controllers;

[CheckPermissions("Roles")]
public class RolesController : MaintenanceController
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

    [HttpPost]
    [CheckPermissions("MaintainRoles")]
    public async Task<IActionResult> CreateRole(CreateRoleDto createRoleDto)
    {

        var role = new Role
        {
            Name = createRoleDto.Name,
        };

        _db.Roles.Add(role);
        await _db.SaveChangesAsync();

        return Created();
    }

    [HttpPut("{roleId:int}/toggledeleted")]
    [CheckPermissions("MaintainRoles")]
    public async Task<IActionResult> ToggleDeleted(int roleId)
    {
        var role = await _db.Roles.FindAsync(roleId);

        if (role == null) return NotFound();

        role.IsDeleted = !role.IsDeleted;
        _db.Roles.Update(role);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{roleId:int}/permissions")]
    public async Task<IActionResult> GetPermissionsByRoleId(int roleId)
    {
        var userRoles = await _db.Permissions
            .Select(p => new RolePermissionDto
            {
                Id = p.Id,
                Name = p.Name,
                ParentPermissionId = p.ParentPermissionId,
                IsAssigned = _db.RolePermissions
                    .Any(rp => rp.RoleId == roleId && rp.PermissionId == p.Id)
            })
            .AsNoTracking()
            .ToListAsync();

        return Ok(userRoles);
    }

    [HttpPut("{roleId:int}/togglepermission/{permissionId:int}")]
    [CheckPermissions("MaintainRoles")]
    public async Task<IActionResult> TogglePermission(int roleId, int permissionId)
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
            _db.RolePermissions.Remove(rolePermission);
        }
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
