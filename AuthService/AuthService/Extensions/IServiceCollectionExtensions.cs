using AuthService.BackgroundServices;
using AuthService.Config;
using AuthService.Data;
using AuthService.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Extensions;

public static class IServiceCollectionExtensions
{
    public static void ConfigureDatabase(this IServiceCollection services, IConfiguration configuration) =>
        services.AddDbContext<AuthDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("AuthDb")));

    public static void ConfigureOptions(this IServiceCollection services) =>
        services.AddOptions<JwtOptions>().BindConfiguration(JwtOptions.Section);

    public static void ConfigureSingletonServices(this IServiceCollection services)
    {
        services.AddSingleton<IPasswordHasher<object>, PasswordHasher<object>>();
        services.AddSingleton<PasswordVerificationService>();
    }
    
    public static void ConfigureHostedServices(this IServiceCollection services) =>
        services.AddHostedService<RefreshTokenCleanupService>();
}
