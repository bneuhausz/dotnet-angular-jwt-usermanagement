using AuthService.Data;
using AuthService.Dtos;
using AuthService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace AuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthDbContext _db;
    private readonly IConfiguration _config;
    private readonly PasswordVerificationService _passwordVerificationService;

    public AuthController(AuthDbContext db, IConfiguration config, PasswordVerificationService passwordVerificationService)
    {
        _db = db;
        _config = config;
        _passwordVerificationService = passwordVerificationService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.UserName == dto.UserName && !u.IsDeleted);

        if (user == null || !_passwordVerificationService.VerifyPassword(user.PasswordHash, dto.Password))
            return Unauthorized();

        var userRoles = await _db.UserRoles
            .Where(ur => ur.UserId == user.Id && !ur.IsDeleted)
            .Select(ur => ur.RoleId)
            .ToListAsync();

        var permissions = await _db.RolePermissions
            .Where(rp => userRoles.Contains(rp.RoleId) && !rp.IsDeleted)
            .Select(rp => rp.Permission.Name)
            .ToListAsync();

        var claims = new List<Claim>
        {
            new Claim("sub", user.Id.ToString()),
            new Claim("name", user.UserName),
            new Claim("email", user.Email),
            new Claim("permissions", JsonSerializer.Serialize(permissions.Distinct())),
        };

        //TODO: implement options pattern and handle config better
        var secret = Encoding.UTF8.GetBytes(_config["Jwt:Secret"]!);
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            issuer: "auth-service",
            audience: "user-manager-api",
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(secret),
                SecurityAlgorithms.HmacSha256)
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new { token = jwt });
    }
}
