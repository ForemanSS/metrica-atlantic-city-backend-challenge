using AtlanticCity.BuildingBlocks.Contracts.MassiveLoad;
using AtlanticCity.BuildingBlocks.Messaging;
using AtlanticCity.Control.Application.Messaging;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace AtlanticCity.Control.Infrastructure.Messaging;

internal sealed class MassTransitLoadProcessingPublisher(
    ISendEndpointProvider sendEndpointProvider,
    ILogger<MassTransitLoadProcessingPublisher> logger)
    : ILoadProcessingPublisher
{
    public async Task PublishAsync(
        Guid loadId,
        string storagePath,
        string fileName,
        string userId,
        string userEmail,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        var endpoint =
            await sendEndpointProvider
                .GetSendEndpoint(
                    new Uri(
                        $"queue:{QueueNames.MassiveLoad}?type=direct"));

        var message =
            new ProcessMassiveLoad(
                loadId,
                storagePath,
                fileName,
                userId,
                userEmail,
                correlationId,
                DateTimeOffset.UtcNow);

        await endpoint.Send(
            message,
            context =>
            {
                context.MessageId =
                    loadId;

                context.Headers.Set(
                    "X-Correlation-Id",
                    correlationId);

                if (Guid.TryParse(
                        correlationId,
                        out var correlationGuid))
                {
                    context.CorrelationId =
                        correlationGuid;
                }
            },
            cancellationToken);

        logger.LogInformation(
            "Massive load message sent. LoadId {LoadId}, Queue {Queue}, CorrelationId {CorrelationId}",
            loadId,
            QueueNames.MassiveLoad,
            correlationId);
    }
}