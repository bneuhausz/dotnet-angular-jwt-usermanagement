using Audit.EntityFramework;
using Microsoft.AspNetCore.HttpLogging;
using UserManagerApi.Data.Audit;
using UserManagerApi.Data;
using UserManagerApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UserManagerApi.Config;
using UserManagerApi.BackgroundServices;

namespace UserManagerApi.Extensions;

public static class IServiceCollectionExtensions
{
    public static void ConfigureHttpLogging(this IServiceCollection services)
    {
        services.AddHttpLogging(logging =>
        {
            logging.LoggingFields = HttpLoggingFields.All;
            logging.MediaTypeOptions.AddText("application/json");
        });
    }

    public static void ConfigureScopedServices(this IServiceCollection services) =>
        services.AddScoped<ICurrentUserService, CurrentUserService>();

    public static void ConfigureDatabaseContexts(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<UsersDbContext>(options => options
            .UseSqlServer(configuration.GetConnectionString("UsersDb"))
            .AddInterceptors(new AuditSaveChangesInterceptor()));

        services.AddDbContext<AuditLogDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("UsersDb")));
    }

    public static void ConfigureSingletonServices(this IServiceCollection services)
    {
        services.AddSingleton<IPasswordHasher<object>, PasswordHasher<object>>();
        services.AddSingleton<PasswordVerificationService>();
    }

    public static void ConfigureAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtOptions = configuration.GetSection("Jwt").Get<JwtOptions>();

        services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                var secret = Encoding.UTF8.GetBytes(jwtOptions!.Secret);
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwtOptions!.Issuer,
                    ValidAudience = jwtOptions!.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(secret),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true
                };
            });

        services.AddAuthorization();
    }

    public static void ConfigureHostedServices(this IServiceCollection services) =>
        services.AddHostedService<AuditLogCleanupService>();
}
