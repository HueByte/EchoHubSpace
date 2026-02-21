using EchoHub.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EchoHub.App.Filters;

/// <summary>
/// Authorization filter that validates the <c>X-Api-Key</c> header against the configured API key.
/// Apply to actions or controllers that require API key authentication.
/// </summary>
public class ApiKeyAuthFilter(IConfiguration configuration) : IAuthorizationFilter
{
    private const string ApiKeyHeaderName = "X-Api-Key";

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var providedKey))
        {
            context.Result = new UnauthorizedObjectResult(ApiResponse.Fail("API key is required"));
            return;
        }

        var configuredKey = configuration["ApiKey"];
        if (string.IsNullOrEmpty(configuredKey) || !string.Equals(configuredKey, providedKey, StringComparison.Ordinal))
        {
            context.Result = new UnauthorizedObjectResult(ApiResponse.Fail("Invalid API key"));
        }
    }
}

/// <summary>
/// Attribute to apply API key authorization to a controller or action.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ApiKeyAuthAttribute : TypeFilterAttribute
{
    public ApiKeyAuthAttribute() : base(typeof(ApiKeyAuthFilter)) { }
}
