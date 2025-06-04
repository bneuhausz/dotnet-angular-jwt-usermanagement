using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UserManagerApi.Services;
using UserManagerApi.Middlewares;
using Microsoft.AspNetCore.HttpLogging;
using Serilog;
using Audit.EntityFramework;
using Audit.Core;
using System.Text.Json;
using System.Diagnostics;
using UserManagerApi.Data.Audit;
using UserManagerApi.Data;
using UserManagerApi.BackgroundServices;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.All;
    logging.MediaTypeOptions.AddText("application/json");
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddDbContext<UsersDbContext>(options => options
    .UseSqlServer(builder.Configuration.GetConnectionString("UsersDb"))
    .AddInterceptors(new AuditSaveChangesInterceptor()));

builder.Services.AddDbContext<AuditLogDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("UsersDb")));

builder.Services.AddSingleton<IPasswordHasher<object>, PasswordHasher<object>>();
builder.Services.AddSingleton<PasswordVerificationService>();


builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        var secret = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!);
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = "auth-service",
            ValidAudience = "user-manager-api",
            IssuerSigningKey = new SymmetricSecurityKey(secret),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddHostedService<AuditLogCleanupService>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var serviceProvider = app.Services;
var auditDbOptions = new DbContextOptionsBuilder<AuditLogDbContext>()
    .UseSqlServer(app.Configuration.GetConnectionString("UsersDb"))
    .Options;

Audit.Core.Configuration.Setup().UseEntityFramework(ef => ef
    .UseDbContext<AuditLogDbContext>(auditDbOptions)
    .DisposeDbContext()
    .AuditTypeMapper(t => typeof(AuditLog))
    .AuditEntityAction<AuditLog>((auditEvent, eventEntry, auditLogEntity) =>
    {
        auditLogEntity.InsertedDate = DateTime.UtcNow;
        auditLogEntity.EntityType = eventEntry.EntityType.Name;
        auditLogEntity.TableName = eventEntry.Table;
        auditLogEntity.PrimaryKey = JsonSerializer.Serialize(eventEntry.PrimaryKey);
        auditLogEntity.Action = eventEntry.Action;

        if (eventEntry.Action == "Insert" || eventEntry.Action == "Delete")
        {
            auditLogEntity.Changes = JsonSerializer.Serialize(eventEntry.ColumnValues);
        }
        else
        {
            auditLogEntity.Changes = JsonSerializer.Serialize(eventEntry.Changes);
        }

        if (auditEvent.CustomFields.TryGetValue("UserId", out var userIdObj) && userIdObj is int userIdVal)
        {
            auditLogEntity.UserId = userIdVal;
        }

        if (auditEvent.CustomFields.TryGetValue("TraceId", out var traceIdObj) && traceIdObj is string traceIdVal)
        {
            auditLogEntity.TraceId = traceIdVal;
        }
    })
);

Audit.Core.Configuration.AddCustomAction(ActionType.OnScopeCreated, scope =>
{
    var httpContextAccessor = app.Services.GetService<IHttpContextAccessor>();
    var httpContext = httpContextAccessor?.HttpContext;

    if (httpContext != null &&
        httpContext.RequestServices.GetService<ICurrentUserService>() is ICurrentUserService currentUserService)
    {
        var userId = currentUserService.GetCurrentUserId();
        scope.Event.CustomFields["UserId"] = userId;
    }

    if (httpContext?.Items.TryGetValue(CorrelationIdMiddleware.CorrelationIdItemName, out var correlationIdObj) == true &&
        correlationIdObj is string corrIdStr &&
        !string.IsNullOrWhiteSpace(corrIdStr))
    {
        scope.Event.CustomFields["TraceId"] = corrIdStr;
    }
    else if (httpContext != null && !string.IsNullOrEmpty(httpContext.TraceIdentifier))
    {
        scope.Event.CustomFields["TraceId"] = httpContext.TraceIdentifier;
    }
    else if (Activity.Current?.Id != null)
    {
        scope.Event.CustomFields["TraceId"] = Activity.Current.Id;
    }
});

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseHttpLogging();

app.UseAuthentication();

app.UseMiddleware<UserContextMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
