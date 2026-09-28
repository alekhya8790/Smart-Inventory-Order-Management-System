using Microsoft.EntityFrameworkCore;

namespace SmartIOMS.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger)
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
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex,
                    "Concurrency conflict occurred.");

                context.Response.StatusCode = StatusCodes.Status409Conflict;

                await context.Response.WriteAsJsonAsync(new
                {
                    statusCode = 409,
                    message = "Stock was updated by another order. Please refresh and try again."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unhandled exception occurred.");

                context.Response.StatusCode =
                    StatusCodes.Status400BadRequest;

                await context.Response.WriteAsJsonAsync(new
                {
                    statusCode = 400,
                    message = ex.Message
                });
            }
        }
    }
}