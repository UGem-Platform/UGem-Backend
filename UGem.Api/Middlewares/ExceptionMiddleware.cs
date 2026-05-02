using System.Net;
using System.Text.Json;

namespace UGem.Api.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        int statusCode;
        string message = exception.Message;

        switch (exception)
        {
            case UnauthorizedAccessException:
                statusCode = (int)HttpStatusCode.Unauthorized;
                break;

            case KeyNotFoundException:
                statusCode = (int)HttpStatusCode.NotFound;
                break;

            case InvalidOperationException:
                statusCode = (int)HttpStatusCode.BadRequest;
                break;

            default:
                statusCode = (int)HttpStatusCode.InternalServerError;
                break;
        }

        context.Response.StatusCode = statusCode;

        var response = new
        {
            success = false,
            statusCode,
            message,
            traceId = context.TraceIdentifier
        };

        var json = JsonSerializer.Serialize(response);

        await context.Response.WriteAsync(json);
    }
}