
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Linq;

public class CustomRoutingMiddleware
{
    private readonly RequestDelegate _next;
    private const string SystemKeyHeader = "systemkey";
    private const string TenantKeyHeader = "tenantkey";
    private const string SubTenantKeyHeader = "subtenantkey";

    public CustomRoutingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Add required Headers to pass along to the controlers.
    /// "systemkey"
    /// "tenantkey"
    /// "subtenantkey"
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value;

        context.Request.Headers.Remove(SystemKeyHeader);
        context.Request.Headers.Remove(TenantKeyHeader);
        context.Request.Headers.Remove(SubTenantKeyHeader);

        var SystemKeyValue = Environment.GetEnvironmentVariable("SYSTEMKEY");
        var TenantKeyValue = Environment.GetEnvironmentVariable("TENANTKEY");
        var SubtenantKeyValue = Environment.GetEnvironmentVariable("SUBTENANTKEY");

        // Add headers with new values
        context.Request.Headers.Add(SystemKeyHeader, SystemKeyValue);
        context.Request.Headers.Add(TenantKeyHeader, TenantKeyValue);
        context.Request.Headers.Add(SubTenantKeyHeader, SubtenantKeyValue);

        await _next(context);
    }
}

// Extension method used to add the middleware to the HTTP request pipeline.
public static class CustomRoutingMiddlewareExtensions
{
    public static IApplicationBuilder UseCustomRouting(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CustomRoutingMiddleware>();
    }
}

