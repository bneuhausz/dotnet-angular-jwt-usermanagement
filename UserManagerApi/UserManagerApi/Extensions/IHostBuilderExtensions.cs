using Serilog;

namespace UserManagerApi.Extensions;

public static class IHostBuilderExtensions
{
    public static void ConfigureLogging(this IHostBuilder host, IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        host.UseSerilog();
    }
}
