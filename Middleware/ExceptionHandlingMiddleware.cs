using System.Text.Json;
using BookingSystemAPI.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystemAPI.Middleware
{
    public class ExceptionHandlingMiddleware
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
            try
            {
                await _next(context);
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                // The browser closed the connection. Nothing to respond to.
                _logger.LogInformation("The client cancelled the request to {Path}.", context.Request.Path);
            }
            catch (Exception ex)
            {
                if (context.Response.HasStarted)
                {
                    _logger.LogError(ex, "An error occurred after the response had started.");
                    throw;
                }

                await WriteProblemAsync(context, ex);
            }
        }

        private async Task WriteProblemAsync(HttpContext context, Exception ex)
        {
            var (status, title, detail) = ex switch
            {
                BusinessRuleException => (StatusCodes.Status400BadRequest, "Request not valid", ex.Message),
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized", ex.Message),
                ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden", ex.Message),
                NotFoundException => (StatusCodes.Status404NotFound, "Not found", ex.Message),
                ConflictException => (StatusCodes.Status409Conflict, "Conflict", ex.Message),
                _ => (StatusCodes.Status500InternalServerError, "Server error",
                      "An unexpected error occurred. Please try again later.")
            };

            if (status >= 500)
            {
                _logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
            }
            else
            {
                _logger.LogInformation("{Title}: {Detail} ({Method} {Path})", title, detail, context.Request.Method, context.Request.Path);
            }

            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail,
                Instance = context.Request.Path
            };

            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(problem, (JsonSerializerOptions?)null, "application/problem+json");
        }
    }
}