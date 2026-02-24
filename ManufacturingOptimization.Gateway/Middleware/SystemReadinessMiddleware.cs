using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Gateway.Exceptions;

namespace ManufacturingOptimization.Gateway.Middleware
{
    public class SystemReadinessMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SystemReadinessMiddleware> _logger;

        public SystemReadinessMiddleware(RequestDelegate next, ILogger<SystemReadinessMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ISystemReadinessService readinessService)
        {
            // Allow notification endpoints to work before system is fully ready
            if (context.Request.Path.StartsWithSegments("/api/notifications"))
            {
                await _next(context);
                return;
            }

            // Check if system is ready
            if (!readinessService.IsSystemReady || !readinessService.IsProvidersReady)
            {
                throw new ServiceNotReadyException();
            }

            await _next(context);
        }
    }

    public static class SystemReadinessMiddlewareExtensions
    {
        public static IApplicationBuilder UseSystemReadiness(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SystemReadinessMiddleware>();
        }
    }
}
