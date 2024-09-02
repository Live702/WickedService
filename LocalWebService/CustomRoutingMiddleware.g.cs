
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

public class CustomRoutingMiddleware
{
    private readonly RequestDelegate _next;

    public CustomRoutingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Remove the /apiPrefix/containerPrefix/ from the path.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value;
        var newPath = "/" + string.Join("/", path.Split('/').Skip(3));
        context.Request.Path = "/api" + newPath;
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

