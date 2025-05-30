using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserManagerApi.Data;
using UserManagerApi.Dtos;
using UserManagerApi.Services;

namespace UserManagerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
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

    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> Get(Guid userId)
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

    [HttpGet("{userId:guid}/roles")]
    public async Task<IActionResult> GetRolesByUserId(Guid userId)
    {
        var userRoles = await _db.Roles
            .Where(r => !r.IsDeleted)
            .Select(r => new UserRoleDto
            {
                Id = r.Id,
                Name = r.Name,
                IsAssigned = _db.UserRoles
                    .Any(ur => ur.UserId == userId && ur.RoleId == r.Id && !ur.IsDeleted)
            })
            .AsNoTracking()
            .ToListAsync();

        return Ok(userRoles);
    }

    [HttpPost]
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

    [HttpPatch("{userId:guid}")]
    public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] PatchUserDto patchUserDto)
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

    [HttpPut("{userId:guid}/toggledeleted")]
    public async Task<IActionResult> ToggleDeleted(Guid userId)
    {
        var user = await _db.Users.FindAsync(userId);

        if (user == null) return NotFound();

        user.IsDeleted = !user.IsDeleted;
        _db.Users.Update(user);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("{userId:guid}/togglerole/{roleId}")]
    public async Task<IActionResult> ToggleRole(Guid userId, Guid roleId)
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
            userRole.IsDeleted = !userRole.IsDeleted;
            _db.UserRoles.Update(userRole);
            await _db.SaveChangesAsync();
        }

        return NoContent();
    }
}
