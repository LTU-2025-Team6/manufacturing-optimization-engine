using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Gateway.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Text.Json;

namespace ManufacturingOptimization.Gateway.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly ProblemDetailsFactory _problemDetailsFactory;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IWebHostEnvironment environment,
            ProblemDetailsFactory problemDetailsFactory)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
            _problemDetailsFactory = problemDetailsFactory;
        }

        public async Task InvokeAsync(HttpContext context, ISystemReadinessService readinessService)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                int statusCode;
                string detail;

                if (ex is GatewayException gatewayEx)
                {
                    statusCode = gatewayEx.StatusCode;
                    detail = gatewayEx.Message;
                }
                else
                {
                    statusCode = StatusCodes.Status500InternalServerError;
                    detail = _environment.IsDevelopment()
                        ? ex.ToString()
                        : "An unexpected error occurred.";
                }

                _logger.LogError(ex, "Unhandled exception occurred while processing request to {Path}", context.Request.Path);

                var problemDetails = _problemDetailsFactory.CreateProblemDetails(
                    context,
                    statusCode,
                    title: "An error occurred while processing your request.",
                    detail: detail,
                    instance: context.Request.Path);

                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(problemDetails);
            }
        }
    }

    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
