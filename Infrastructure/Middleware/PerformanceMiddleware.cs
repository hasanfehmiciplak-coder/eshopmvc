using System.Diagnostics;
using Serilog;

namespace EShopMVC.Infrastructure.Middleware
{
    public class PerformanceMiddleware
    {
        private readonly RequestDelegate _next;

        public PerformanceMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            await _next(context);

            stopwatch.Stop();

            var path = context.Request.Path;
            var method = context.Request.Method;
            var elapsed = stopwatch.ElapsedMilliseconds;

            Log.Information(
                "Request {Method} {Path} completed in {Elapsed} ms",
                method,
                path,
                elapsed
            );

            if (elapsed > 1000)
            {
                Log.Warning(
                    "Slow request detected: {Method} {Path} took {Elapsed} ms",
                    method,
                    path,
                    elapsed
                );
            }
        }
    }
}