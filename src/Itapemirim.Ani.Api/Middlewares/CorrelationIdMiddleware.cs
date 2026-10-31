namespace Itapemirim.Ani.Api.Middlewares;

using Microsoft.AspNetCore.Http;

public class CorrelationIdMiddleware
{
    private const string HeaderKey = "X-Correlation-ID";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(HeaderKey, out var correlationId) || !Guid.TryParse(correlationId, out var parsed))
        {
            parsed = Guid.NewGuid();
        }

        context.Items["CorrelationId"] = parsed;
        context.Response.Headers[HeaderKey] = parsed.ToString();

        await _next(context);
    }
}