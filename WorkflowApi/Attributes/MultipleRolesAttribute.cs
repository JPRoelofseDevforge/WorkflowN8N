using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using WorkflowApi.Services;

namespace WorkflowApi.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class MultipleRolesAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _roleNames;

    public MultipleRolesAttribute(params string[] roleNames)
    {
        _roleNames = roleNames;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var authorizationService = context.HttpContext.RequestServices.GetService(typeof(AuthorizationService)) as AuthorizationService;
        if (authorizationService == null)
        {
            context.Result = new Microsoft.AspNetCore.Mvc.UnauthorizedResult();
            return;
        }

        foreach (var roleName in _roleNames)
        {
            var hasRole = authorizationService.HasRoleAsync(roleName).GetAwaiter().GetResult();
            if (hasRole)
            {
                return; // Success
            }
        }

        context.Result = new Microsoft.AspNetCore.Mvc.ForbidResult();
    }
}