using Microsoft.AspNetCore.Authorization;
using WorkflowApi.Services;

namespace WorkflowApi.Services;

public class MultipleRolesHandler : AuthorizationHandler<MultipleRolesRequirement>
{
    private readonly AuthorizationService _authorizationService;

    public MultipleRolesHandler(AuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        MultipleRolesRequirement requirement)
    {
        foreach (var roleName in requirement.RoleNames)
        {
            if (await _authorizationService.HasRoleAsync(roleName))
            {
                context.Succeed(requirement);
                return;
            }
        }
    }
}

public class MultipleRolesRequirement : IAuthorizationRequirement
{
    public string[] RoleNames { get; }

    public MultipleRolesRequirement(string[] roleNames)
    {
        RoleNames = roleNames;
    }
}