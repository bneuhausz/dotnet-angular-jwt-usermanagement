namespace UserManagerApi.Middlewares;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;
    private const string CorrelationIdHeaderName = "X-Correlation-ID";
    public const string CorrelationIdItemName = "CorrelationId";

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var correlationId = Guid.NewGuid().ToString();

        context.Items[CorrelationIdHeaderName] = correlationId;

        using (Serilog.Context.LogContext.PushProperty(CorrelationIdItemName, correlationId))
        using (_logger.BeginScope(new Dictionary<string, object> { [CorrelationIdItemName] = correlationId }))
        {
            await _next(context);
        }
    }
}
