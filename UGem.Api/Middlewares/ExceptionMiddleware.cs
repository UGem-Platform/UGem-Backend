using System.Text.Json;
using UGem.Services.Models;

namespace UGem.Api.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger)
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
        catch (Exception ex)
        {
            // LOG FULL ERROR
            _logger.LogError(ex,
                "Unhandled Exception. TraceId: {TraceId}",
                context.TraceIdentifier);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        context.Response.ContentType = "application/json";

        int statusCode;
        string message = exception.Message;

        switch (exception)
        {
            case UnauthorizedAccessException:
                statusCode = StatusCodes.Status401Unauthorized;
                break;

            case KeyNotFoundException:
                statusCode = StatusCodes.Status404NotFound;
                break;

            case InvalidOperationException:
                statusCode = StatusCodes.Status400BadRequest;
                break;

            default:
                statusCode = StatusCodes.Status500InternalServerError;
                break;
        }

        context.Response.StatusCode = statusCode;

        var errorCode = statusCode switch
        {
            StatusCodes.Status400BadRequest => "bad_request",
            StatusCodes.Status401Unauthorized => "unauthorized",
            StatusCodes.Status404NotFound => "not_found",
            _ => "internal_server_error"
        };

        object? details = null;
#if DEBUG
        details = exception.ToString();
#endif

        var response = ApiResponseFactory.ErrorResponse(
            message,
            new
            {
                code = errorCode,
                details
            },
            context.TraceIdentifier);

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
