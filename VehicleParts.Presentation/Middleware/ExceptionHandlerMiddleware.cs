using System.Text.Json;

namespace VehicleParts.Presentation.Middleware;

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred.");

            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = 500;

            await httpContext.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                message = "Internal server error",
                statusCode = 500
            }));
        }
    }
}