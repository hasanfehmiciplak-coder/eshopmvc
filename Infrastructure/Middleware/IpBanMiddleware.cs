using EShopMVC.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class IpBanMiddleware
{
    private readonly RequestDelegate _next;

    public IpBanMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, AppDbContext db)
    {
        var ip = context.Connection.RemoteIpAddress?.ToString();

        if (!string.IsNullOrEmpty(ip))
        {
            var banned = await db.BannedIps
                .AnyAsync(x => x.IpAddress == ip && x.IsActive);

            if (banned)
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("IP adresiniz engellenmiştir.");
                return;
            }
        }

        await _next(context);
    }
}