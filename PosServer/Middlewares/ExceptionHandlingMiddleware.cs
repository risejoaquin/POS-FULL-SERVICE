using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using System.Text.Json;
using PosServer.Models;
using Serilog;

namespace PosServer.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
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

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var code = "INTERNAL_SERVER_ERROR";
            var statusCode = StatusCodes.Status500InternalServerError;
            var message = "Ha ocurrido un error inesperado en el servidor.";

            if (exception is InvalidOperationException)
            {
                statusCode = StatusCodes.Status422UnprocessableEntity;
                code = "BUSINESS_RULE_VIOLATION";
                message = exception.Message;
            }
            else if (exception is UnauthorizedAccessException)
            {
                statusCode = StatusCodes.Status403Forbidden;
                code = "FORBIDDEN";
                message = exception.Message;
            }
            
            var correlationId = context.Request.Headers["X-Correlation-ID"].ToString();
            if (string.IsNullOrEmpty(correlationId))
            {
                correlationId = context.Response.Headers["X-Correlation-ID"].ToString();
            }

            Log.Error(exception, "Error {Code}: {Message} (CorrelationId: {CorrelationId})", code, message, correlationId);

            var result = JsonSerializer.Serialize(new ErrorResponse
            {
                Code = code,
                Message = message,
                CorrelationId = correlationId
            });

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            return context.Response.WriteAsync(result);
        }
    }
}
