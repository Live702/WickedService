
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

        // Add headers with new values
        context.Request.Headers.Add(SystemKeyHeader, "lzm");
        context.Request.Headers.Add(TenantKeyHeader, "lzm-mp");
        context.Request.Headers.Add(SubTenantKeyHeader, "lzm-mp-uptown");

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

