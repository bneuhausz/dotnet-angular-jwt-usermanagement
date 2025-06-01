using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserManagerApi.Attributes;
using UserManagerApi.Data;
using UserManagerApi.Dtos;
using UserManagerApi.Services;

namespace UserManagerApi.Controllers;

[CheckPermissions("Users")]
public class UsersController : MaintenanceController
{
    private readonly UsersDbContext _db;
    private readonly PasswordVerificationService _passwordVerificationService;

    public UsersController(UsersDbContext db, PasswordVerificationService passwordVerificationService)
    {
        _db = db;
        _passwordVerificationService = passwordVerificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _db.Users
            .Select(u => new UserDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                IsDeleted = u.IsDeleted,
            })
            .AsNoTracking()
            .ToListAsync();
        return Ok(users);
    }

    [HttpGet("{userId:int}")]
    public async Task<IActionResult> Get(int userId)
    {
        var user = await _db.Users.FindAsync(userId);

        if (user == null) return NotFound();

        return Ok(new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email
        });
    }

    [HttpGet("{userId:int}/roles")]
    public async Task<IActionResult> GetRolesByUserId(int userId)
    {
        var userRoles = await _db.Roles
            .Where(r => !r.IsDeleted)
            .Select(r => new UserRoleDto
            {
                Id = r.Id,
                Name = r.Name,
                IsAssigned = _db.UserRoles
                    .Any(ur => ur.UserId == userId && ur.RoleId == r.Id)
            })
            .AsNoTracking()
            .ToListAsync();

        return Ok(userRoles);
    }

    [HttpPost]
    [CheckPermissions("MaintainUsers")]
    public async Task<IActionResult> CreateUser(CreateUserDto createUserDto)
    {
        var user = new User
        {
            UserName = createUserDto.UserName,
            Email = createUserDto.Email,
            PasswordHash = _passwordVerificationService.HashPassword(createUserDto.Password),
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Created();
    }

    [HttpPatch("{userId:int}")]
    [CheckPermissions("MaintainUsers")]
    public async Task<IActionResult> UpdateUser(int userId, [FromBody] PatchUserDto patchUserDto)
    {
        var user = await _db.Users.FindAsync(userId);

        if (user == null) return NotFound();

        if (patchUserDto.UserName != null)
            user.UserName = patchUserDto.UserName;

        if (patchUserDto.Email != null)
            user.Email = patchUserDto.Email;

        if (patchUserDto.Password != null)
            user.PasswordHash = _passwordVerificationService.HashPassword(patchUserDto.Password);

        _db.Users.Update(user);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("{userId:int}/toggledeleted")]
    [CheckPermissions("MaintainUsers")]
    public async Task<IActionResult> ToggleDeleted(int userId)
    {
        var user = await _db.Users.FindAsync(userId);

        if (user == null) return NotFound();

        user.IsDeleted = !user.IsDeleted;
        _db.Users.Update(user);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("{userId:int}/togglerole/{roleId:int}")]
    [CheckPermissions("MaintainUsers")]
    public async Task<IActionResult> ToggleRole(int userId, int roleId)
    {
        var userRole = _db.UserRoles
            .FirstOrDefault(ur => ur.UserId == userId && ur.RoleId == roleId);

        if (userRole == null)
        {
            _db.UserRoles.Add(new UserRole
            {
                UserId = userId,
                RoleId = roleId,
            });
            await _db.SaveChangesAsync();
        }
        else
        {
            _db.UserRoles.Remove(userRole);
            await _db.SaveChangesAsync();
        }

        return NoContent();
    }
}
