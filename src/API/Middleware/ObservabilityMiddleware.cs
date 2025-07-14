using Serilog;
using System.Diagnostics;

namespace API.Middleware
{
    public class ObservabilityMiddleware
    {
        private readonly RequestDelegate _next;

        public ObservabilityMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                Log.Information("Incoming request: {Method} {Path}", context.Request.Method, context.Request.Path);

                await _next(context);

                stopwatch.Stop();
                Log.Information("Handled {Method} {Path} in {Elapsed} ms with status {StatusCode}",
                    context.Request.Method,
                    context.Request.Path,
                    stopwatch.ElapsedMilliseconds,
                    context.Response.StatusCode);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Log.Error(ex, "Unhandled exception in {Method} {Path} after {Elapsed} ms",
                    context.Request.Method,
                    context.Request.Path,
                    stopwatch.ElapsedMilliseconds);
                throw;
            }
        }
    }
}
