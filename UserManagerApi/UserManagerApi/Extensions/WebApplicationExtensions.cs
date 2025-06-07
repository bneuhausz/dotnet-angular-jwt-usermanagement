using Audit.Core;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Text.Json;
using UserManagerApi.Data.Audit;
using UserManagerApi.Middlewares;
using UserManagerApi.Services;

namespace UserManagerApi.Extensions;

public static class WebApplicationExtensions
{
    public static void ConfigureAudit(this WebApplication app)
    {
        var auditDbOptions = new DbContextOptionsBuilder<AuditLogDbContext>()
            .UseSqlServer(app.Configuration.GetConnectionString("UsersDb"))
            .Options;

        Configuration.Setup().UseEntityFramework(ef => ef
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

        Configuration.AddCustomAction(ActionType.OnScopeCreated, scope =>
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
    }
}
