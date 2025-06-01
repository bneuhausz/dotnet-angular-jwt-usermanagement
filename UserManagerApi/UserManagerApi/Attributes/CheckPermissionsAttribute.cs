using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Reflection;
using System.Text.Json;

namespace UserManagerApi.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class CheckPermissionsAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string[] _requiredPermissions;

    public CheckPermissionsAttribute(params string[] requiredPermissions)
    {
        _requiredPermissions = requiredPermissions;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new ForbidResult();
            return;
        }

        var allRequiredPermissions = GetAllPermissions(context);

        var permissionsClaim = user.FindFirst("permissions");

        HashSet<string> userPermissions = new();

        if (permissionsClaim is not null)
        {
            userPermissions = JsonSerializer
                .Deserialize<List<string>>(permissionsClaim.Value)?
                .ToHashSet(StringComparer.OrdinalIgnoreCase)
                ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        var missingPermissions = allRequiredPermissions.Where(p => !userPermissions.Contains(p)).ToList();

        if (missingPermissions.Any())
        {
            context.Result = new ForbidResult();
        }

        await Task.CompletedTask;
    }

    private static List<string> GetAllPermissions(AuthorizationFilterContext context)
    {
        var allAttributes = new List<CheckPermissionsAttribute>();

        var actionAttributes = context.ActionDescriptor.EndpointMetadata
            .OfType<CheckPermissionsAttribute>();
        allAttributes.AddRange(actionAttributes);

        if (context.ActionDescriptor is ControllerActionDescriptor cad)
        {
            var controllerType = cad.ControllerTypeInfo;
            while (controllerType != null && controllerType != typeof(object))
            {
                var classAttributes = controllerType
                    .GetCustomAttributes(typeof(CheckPermissionsAttribute), inherit: false)
                    .OfType<CheckPermissionsAttribute>();

                allAttributes.AddRange(classAttributes);

                controllerType = controllerType.BaseType?.GetTypeInfo();
            }
        }

        return allAttributes
            .SelectMany(attr => attr._requiredPermissions)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
