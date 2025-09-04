using Microsoft.AspNetCore.Authorization;
using WorkflowApi.Services;

namespace WorkflowApi.Services;

public class RequireRoleHandler : AuthorizationHandler<RequireRoleRequirement>
{
    private readonly AuthorizationService _authorizationService;

    public RequireRoleHandler(AuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RequireRoleRequirement requirement)
    {
        if (await _authorizationService.HasRoleAsync(requirement.RoleName))
        {
            context.Succeed(requirement);
        }
    }
}

public class RequireRoleRequirement : IAuthorizationRequirement
{
    public string RoleName { get; }

    public RequireRoleRequirement(string roleName)
    {
        RoleName = roleName;
    }
}