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
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var users = await _db.Roles
            .Select(u => new RoleDto
            {
                Id = u.Id,
                Name = u.Name,
                IsDeleted = u.IsDeleted,
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        return Ok(users);
    }

    [HttpPost]
    [CheckPermissions("MaintainRoles")]
    public async Task<IActionResult> CreateRole(CreateRoleDto createRoleDto, CancellationToken cancellationToken)
    {

        var role = new Role
        {
            Name = createRoleDto.Name,
        };

        _db.Roles.Add(role);
        await _db.SaveChangesAsync(cancellationToken);

        return Created();
    }

    [HttpPut("{roleId:int}/toggledeleted")]
    [CheckPermissions("MaintainRoles")]
    public async Task<IActionResult> ToggleDeleted(int roleId, CancellationToken cancellationToken)
    {
        var role = await _db.Roles.FindAsync(roleId, cancellationToken);

        if (role == null) return NotFound();

        role.IsDeleted = !role.IsDeleted;
        _db.Roles.Update(role);
        await _db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpGet("{roleId:int}/permissions")]
    public async Task<IActionResult> GetPermissionsByRoleId(int roleId, CancellationToken cancellationToken)
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
            .ToListAsync(cancellationToken);

        return Ok(userRoles);
    }

    [HttpPut("{roleId:int}/togglepermission/{permissionId:int}")]
    [CheckPermissions("MaintainRoles")]
    public async Task<IActionResult> TogglePermission(int roleId, int permissionId, CancellationToken cancellationToken)
    {
        var rolePermission = await _db.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId, cancellationToken);

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
        await _db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
