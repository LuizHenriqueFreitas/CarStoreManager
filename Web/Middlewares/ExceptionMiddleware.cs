using System.Net;
using System.Text.Json;
using CarStoreManager.Web.Models;

namespace CarStoreManager.Web.Middlewares;

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
            await HandleException(context, ex);
        }
    }

    private static async Task HandleException(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = new ErrorResponse(
            context.Response.StatusCode,
            "Erro interno no servidor",
            exception.Message
        );

        var json = JsonSerializer.Serialize(response);

        await context.Response.WriteAsync(json);
    }
}