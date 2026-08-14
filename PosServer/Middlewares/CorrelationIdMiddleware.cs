using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace PosServer.Middlewares
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private const string CorrelationIdHeader = "X-Correlation-ID";

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = context.Request.Headers[CorrelationIdHeader].ToString();
            
            if (string.IsNullOrEmpty(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
                context.Request.Headers[CorrelationIdHeader] = correlationId; // PHASE 7E ASP.NET header analyzer hygiene applied
            }

            context.Response.OnStarting(() =>
            {
                if (!context.Response.Headers.ContainsKey(CorrelationIdHeader))
                {
                    context.Response.Headers[CorrelationIdHeader] = correlationId; // PHASE 7E ASP.NET header analyzer hygiene applied
                }
                return Task.CompletedTask;
            });

            await _next(context);
        }
    }
}
