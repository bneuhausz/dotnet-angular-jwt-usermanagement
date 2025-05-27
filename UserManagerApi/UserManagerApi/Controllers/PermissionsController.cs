using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserManagerApi.Data;
using UserManagerApi.Dtos;

namespace UserManagerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PermissionsController : ControllerBase
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
            .Where(p => !p.IsDeleted)
            .Select(p => new PermissionDto
            {
                Id = p.Id,
                Name = p.Name,
            })
            .ToListAsync();

        return Ok(permissions);
    }
}
