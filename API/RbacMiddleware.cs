using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SsoBackend.API;

public class RbacMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _requiredRole;

    public RbacMiddleware(RequestDelegate next, string requiredRole)
    {
        _next = next;
        _requiredRole = requiredRole;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var user = context.User;
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized");
            return;
        }
        if (!user.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == _requiredRole))
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("Forbidden");
            return;
        }
        await _next(context);
    }
}
