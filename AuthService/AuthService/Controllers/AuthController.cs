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
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthDbContext _db;
    private readonly IConfiguration _config;
    private readonly PasswordVerificationService _passwordVerificationService;

    private static readonly JsonSerializerOptions CamelCaseJsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

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
            .FirstOrDefaultAsync(u => u.Email == dto.Email && !u.IsDeleted);

        if (user == null || !_passwordVerificationService.VerifyPassword(user.PasswordHash, dto.Password))
            return Unauthorized();

        var userRoles = await _db.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Select(ur => ur.RoleId)
            .ToListAsync();

        var permissions = await _db.RolePermissions
            .Where(rp => userRoles.Contains(rp.RoleId))
            .Select(rp => rp.Permission)
            .ToListAsync();

        var permissionNames = permissions
            .Where(p => p.Type == PermissionType.Action)
            .Select(p => p.Name)
            .Distinct()
            .ToList();

        return Ok(new UserDto { Token = CreateToken(user, permissionNames, CreateMenuTree(permissions)) });
    }

    private List<MenuDto> CreateMenuTree(List<Permission> permissions)
    {
        var userMenus = permissions
            .Where(p => p.Type == PermissionType.Menu)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.ParentPermissionId
            })
            .DistinctBy(p => p.Id)
            .ToList();

        var menuDict = userMenus
            .ToDictionary(p => p.Id, p => new MenuDto
            {
                Name = p.Name,
                SubMenus = []
            });

        List<MenuDto> menus = [];

        foreach (var item in userMenus)
        {
            if (item.ParentPermissionId == null)
            {
                menus.Add(menuDict[item.Id]);
            }
            else if (menuDict.TryGetValue(item.ParentPermissionId.Value, out var parentDto))
            {
                parentDto.SubMenus.Add(menuDict[item.Id]);
            }
        }

        return menus;
    }

    private string CreateToken(User user, List<string> permissionNames, List<MenuDto> menus)
    {
        //TODO: implement options pattern and handle config better
        var secret = Encoding.UTF8.GetBytes(_config["Jwt:Secret"]!);
        var token = new JwtSecurityToken(
            claims: CreateClaims(user, permissionNames, menus),
            //TODO: add to config
            expires: DateTime.UtcNow.AddHours(24),
            issuer: "auth-service",
            audience: "user-manager-api",
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(secret),
                SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private List<Claim> CreateClaims(User user, List<string> permissionNames, List<MenuDto> menus)
    {
        var claims = new List<Claim>
        {
            new Claim("sub", user.Id.ToString()),
            new Claim("name", user.UserName),
            new Claim("email", user.Email),
            new Claim("permissions", JsonSerializer.Serialize(permissionNames)),
            new Claim("menus", JsonSerializer.Serialize(menus, CamelCaseJsonSerializerOptions)),
        };

        return claims;
    }
}
