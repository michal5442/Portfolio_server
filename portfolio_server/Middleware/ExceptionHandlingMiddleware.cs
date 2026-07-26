using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace portfolio_server.Middleware
{
    public sealed class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = ResolveCorrelationId(context);
            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                try
                {
                    await _next(context);
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogWarning(ex, "Validation failure occurred while processing request. CorrelationId: {CorrelationId}", correlationId);
                    await HandleValidationExceptionAsync(context, ex, correlationId); 
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled exception occurred while processing request. CorrelationId: {CorrelationId}", correlationId);
                    await HandleExceptionAsync(context, correlationId);
                }
            }
        }

        private static string ResolveCorrelationId(HttpContext context)
        {
            if (context.Items.TryGetValue(CorrelationIdMiddleware.ItemsKey, out var existing) &&
                existing is string existingId &&
                !string.IsNullOrWhiteSpace(existingId))
            {
                return existingId;
            }

            return Guid.NewGuid().ToString("N");
        }

        private static Task HandleExceptionAsync(HttpContext context, string correlationId)
        {
            if (context.Response.HasStarted)
            {
                return Task.CompletedTask;
            }

            context.Response.Clear();
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";
            context.Response.Headers[CorrelationIdMiddleware.HeaderName] = correlationId;

            var payload = JsonSerializer.Serialize(new { error = "An unexpected error occurred." });
            return context.Response.WriteAsync(payload);
        }

        private static Task HandleValidationExceptionAsync(HttpContext context, InvalidOperationException ex, string correlationId)
        {
            if (context.Response.HasStarted)
            {
                return Task.CompletedTask;
            }

            context.Response.Clear();
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";
            context.Response.Headers[CorrelationIdMiddleware.HeaderName] = correlationId;

            var payload = JsonSerializer.Serialize(new { error = ex.Message });
            return context.Response.WriteAsync(payload);
        }
    }

    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}