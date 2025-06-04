using AuthService.Data;
using AuthService.Dtos;
using AuthService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace AuthService.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthDbContext _db;
    private readonly IConfiguration _config;
    private readonly PasswordVerificationService _passwordVerificationService;
    private readonly ILogger<AuthController> _logger;

    private static readonly JsonSerializerOptions CamelCaseJsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AuthController(AuthDbContext db, IConfiguration config,
        PasswordVerificationService passwordVerificationService, ILogger<AuthController> logger)
    {
        _db = db;
        _config = config;
        _passwordVerificationService = passwordVerificationService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto, CancellationToken cancellationToken)
    {
        _logger.LogInformation("({user}) login attempt", dto.Email);

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email && !u.IsDeleted, cancellationToken);

        if (user == null || !_passwordVerificationService.VerifyPassword(user.PasswordHash, dto.Password))
        {
            if (user == null)
            {
                _logger.LogInformation("({user}) not found", dto.Email);
            }
            else
            {
                _logger.LogInformation("({user}) password verification failed", dto.Email);
            }
            return Unauthorized();
        }

        var accessToken = await CreateAccessToken(user, cancellationToken);
        var refreshToken = GenerateRefreshToken();

        var refreshTokenEntity = new UserRefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:RefreshTokenExpirationMinutes"]!))
        };

        _db.UserRefreshTokens.Add(refreshTokenEntity);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("({user}) logged in", dto.Email);

        return Ok(new UserDto { AccessToken = accessToken, RefreshToken = refreshToken });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenDto dto, CancellationToken cancellationToken)
    {
        var refreshTokenEntity = await _db.UserRefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == dto.RefreshToken, cancellationToken);

        if (refreshTokenEntity == null || !refreshTokenEntity.IsActive)
            return Unauthorized("Invalid or expired refresh token.");

        refreshTokenEntity.RevokedAt = DateTime.UtcNow;
        var newRefreshToken = GenerateRefreshToken();

        var newRefreshTokenEntity = new UserRefreshToken
        {
            UserId = refreshTokenEntity.UserId,
            Token = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:RefreshTokenExpirationMinutes"]!)),
            CreatedAt = DateTime.UtcNow,
            ReplacedByToken = newRefreshToken
        };

        _db.UserRefreshTokens.Add(newRefreshTokenEntity);
        await _db.SaveChangesAsync(cancellationToken);

        var user = refreshTokenEntity.User;
        var accessToken = await CreateAccessToken(user, cancellationToken);

        return Ok(new UserDto
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken
        });
    }

    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke(RefreshTokenDto dto, CancellationToken cancellationToken)
    {
        var refreshTokenEntity = await _db.UserRefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == dto.RefreshToken, cancellationToken);

        if (refreshTokenEntity == null || !refreshTokenEntity.IsActive)
            return NotFound("Token not found or already revoked.");

        refreshTokenEntity.RevokedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return NoContent();
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

    private async Task<string> CreateAccessToken(User user, CancellationToken cancellationToken)
    {
        var userRoles = await _db.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Select(ur => ur.RoleId)
            .ToListAsync(cancellationToken);

        var permissions = await _db.RolePermissions
            .Where(rp => userRoles.Contains(rp.RoleId))
            .Select(rp => rp.Permission)
            .ToListAsync(cancellationToken);

        var permissionNames = permissions
            .Select(p => p.Name)
            .Distinct()
            .ToList();

        var menus = CreateMenuTree(permissions);

        //TODO: implement options pattern and handle config better
        var secret = Encoding.UTF8.GetBytes(_config["Jwt:Secret"]!);

        var token = new JwtSecurityToken(
            claims: CreateClaims(user, permissionNames, menus),
            expires: DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:AccessTokenExpirationMinutes"]!)),
            issuer: _config["Jwt:Issuer"]!,
            audience: _config["Jwt:Audience"]!,
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

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
