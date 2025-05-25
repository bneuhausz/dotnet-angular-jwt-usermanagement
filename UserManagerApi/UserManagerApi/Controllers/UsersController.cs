using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserManagerApi.Data;

namespace UserManagerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly UsersDbContext _db;

    public UsersController(UsersDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _db.Users.ToListAsync();
        return Ok(users);
    }

    [HttpGet("{userId:guid}/roles")]
    public async Task<IActionResult> GetRolesByUserId(Guid userId)
    {
        var userRoles = await _db.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.Role)
            .ToListAsync();

        return Ok(userRoles);
    }
}
