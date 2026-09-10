using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AtlanticCity.BuildingBlocks.Observability;

public sealed class CorrelationIdMiddleware(
    RequestDelegate next,
    ILogger<CorrelationIdMiddleware> logger)
{
    public async Task InvokeAsync(
        HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(
            context);

        var correlationId =
            ResolveCorrelationId(
                context);

        context.TraceIdentifier =
            correlationId;

        context.Request.Headers[
            CorrelationIdHeader.Name] =
            correlationId;

        context.Response.OnStarting(
            () =>
            {
                context.Response.Headers[
                    CorrelationIdHeader.Name] =
                    correlationId;

                return Task.CompletedTask;
            });

        using var scope =
            logger.BeginScope(
                new Dictionary<string, object?>
                {
                    ["CorrelationId"] =
                        correlationId
                });

        var stopwatch =
            Stopwatch.StartNew();

        try
        {
            await next(
                context);

            stopwatch.Stop();

            logger.LogInformation(
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds} ms.",
                context.Request.Method,
                context.Request.Path.Value,
                context.Response.StatusCode,
                stopwatch.Elapsed
                    .TotalMilliseconds);
        }
        catch (Exception exception)
        {
            stopwatch.Stop();

            logger.LogError(
                exception,
                "HTTP {Method} {Path} failed after {ElapsedMilliseconds} ms.",
                context.Request.Method,
                context.Request.Path.Value,
                stopwatch.Elapsed
                    .TotalMilliseconds);

            throw;
        }
    }

    private static string ResolveCorrelationId(
        HttpContext context)
    {
        var incoming =
            context.Request.Headers[
                    CorrelationIdHeader.Name]
                .FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(
                incoming))
        {
            incoming =
                incoming.Trim();

            if (incoming.Length <=
                CorrelationIdHeader.MaxLength)
            {
                return incoming;
            }
        }

        var activityTraceId =
            Activity.Current?
                .TraceId
                .ToString();

        if (!string.IsNullOrWhiteSpace(
                activityTraceId))
        {
            return activityTraceId;
        }

        return Guid.NewGuid()
            .ToString("N");
    }
}