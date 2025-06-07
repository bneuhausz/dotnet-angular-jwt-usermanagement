namespace AuthService.Config;

public class JwtOptions
{
    public const string Section = "Jwt";
    public required string Secret { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public required int AccessTokenExpirationMinutes { get; set; }
    public required int RefreshTokenExpirationMinutes { get; set; }
}
