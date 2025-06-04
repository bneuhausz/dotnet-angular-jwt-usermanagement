using Microsoft.EntityFrameworkCore;
using UserManagerApi.Data.Audit;

namespace UserManagerApi.BackgroundServices;

public class AuditLogCleanupService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<AuditLogCleanupService> _logger;

    public AuditLogCleanupService(IServiceProvider services, ILogger<AuditLogCleanupService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await CleanupAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            var nextRun = now.Date.AddDays(1);

            var delay = nextRun - now;
            _logger.LogInformation("Next audit log cleanup scheduled at {NextRun}", nextRun);

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }

            await CleanupAsync(stoppingToken);
        }
    }

    private async Task CleanupAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AuditLogDbContext>();

            var cutoff = DateTime.UtcNow.AddYears(-1);
            var oldLogs = db.AuditLogs.Where(log => log.InsertedDate < cutoff);

            var count = await oldLogs.CountAsync(stoppingToken);
            db.AuditLogs.RemoveRange(oldLogs);
            await db.SaveChangesAsync(stoppingToken);

            _logger.LogInformation("Audit cleanup ran at {Now}: Deleted {Count} logs older than {Cutoff}",
                DateTime.UtcNow, count, cutoff);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during audit log cleanup");
        }
    }
}
