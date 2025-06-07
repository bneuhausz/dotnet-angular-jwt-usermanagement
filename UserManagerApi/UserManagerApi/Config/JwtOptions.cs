namespace UserManagerApi.Config;

public class JwtOptions
{
    public const string Section = "Jwt";
    public required string Secret { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
}
