using AuthService.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService.BackgroundServices;

public class RefreshTokenCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RefreshTokenCleanupService> _logger;
    private Timer? _timer;

    public RefreshTokenCleanupService(IServiceProvider serviceProvider, ILogger<RefreshTokenCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await CleanUpAsync();

        var now = DateTime.UtcNow;
        var midnightUtc = now.Date.AddDays(1);
        var initialDelay = midnightUtc - now;

        _timer = new Timer(async _ => await RunDailyCleanup(), null, initialDelay, Timeout.InfiniteTimeSpan);
    }

    private async Task RunDailyCleanup()
    {
        try
        {
            await CleanUpAsync();
            _timer?.Change(TimeSpan.FromDays(1), Timeout.InfiniteTimeSpan);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during daily token cleanup.");
        }
    }

    private async Task CleanUpAsync()
    {
        try
        {
            _logger.LogInformation("Starting refresh token cleanup...");

            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

            var cutoffDate = DateTime.UtcNow.AddDays(-30);

            var deletedCount = await db.UserRefreshTokens
                .Where(rt =>
                    (rt.RevokedAt != null || rt.ExpiresAt < DateTime.UtcNow)
                    && rt.CreatedAt < cutoffDate)
                .ExecuteDeleteAsync();

            _logger.LogInformation("Deleted {Count} old refresh tokens.", deletedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up old refresh tokens.");
        }
    }

    public override void Dispose()
    {
        _timer?.Dispose();
        base.Dispose();
    }
}
