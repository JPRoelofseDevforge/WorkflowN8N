using Microsoft.AspNetCore.Authorization;
using WorkflowApi.Services;

namespace WorkflowApi.Services;

public class RequirePermissionHandler : AuthorizationHandler<RequirePermissionRequirement>
{
    private readonly AuthorizationService _authorizationService;

    public RequirePermissionHandler(AuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RequirePermissionRequirement requirement)
    {
        if (await _authorizationService.HasPermissionAsync(requirement.PermissionName))
        {
            context.Succeed(requirement);
        }
    }
}

public class RequirePermissionRequirement : IAuthorizationRequirement
{
    public string PermissionName { get; }

    public RequirePermissionRequirement(string permissionName)
    {
        PermissionName = permissionName;
    }
}