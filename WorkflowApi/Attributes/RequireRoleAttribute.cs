using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using WorkflowApi.Services;

namespace WorkflowApi.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequireRoleAttribute : Attribute, IAuthorizationFilter
{
    private readonly string _roleName;

    public RequireRoleAttribute(string roleName)
    {
        _roleName = roleName;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var authorizationService = context.HttpContext.RequestServices.GetService(typeof(AuthorizationService)) as AuthorizationService;
        if (authorizationService == null)
        {
            context.Result = new Microsoft.AspNetCore.Mvc.UnauthorizedResult();
            return;
        }

        var hasRole = authorizationService.HasRoleAsync(_roleName).GetAwaiter().GetResult();
        if (!hasRole)
        {
            context.Result = new Microsoft.AspNetCore.Mvc.ForbidResult();
        }
    }
}