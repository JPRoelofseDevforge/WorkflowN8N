using System.Net;
using Microsoft.AspNetCore.Authorization;
using WorkflowApi.Services;

namespace WorkflowApi.Services;

public class AuthorizationMiddleware
{
    private readonly RequestDelegate _next;

    public AuthorizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AuthorizationService authorizationService)
    {
        // Check if the endpoint requires authorization
        var endpoint = context.GetEndpoint();
        if (endpoint != null)
        {
            var authorizeData = endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>();
            if (authorizeData.Any())
            {
                // Check if user is authenticated
                if (!context.User.Identity?.IsAuthenticated ?? true)
                {
                    await HandleUnauthorizedAsync(context, "Authentication required");
                    return;
                }

                // Here you can add custom authorization logic
                // For example, check specific permissions based on the endpoint
                // This is a placeholder for custom logic
            }
        }

        await _next(context);
    }

    private async Task HandleUnauthorizedAsync(HttpContext context, string message)
    {
        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync($"{{\"error\": \"{message}\"}}");
    }
}

public static class AuthorizationMiddlewareExtensions
{
    public static IApplicationBuilder UseAuthorizationMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AuthorizationMiddleware>();
    }
}