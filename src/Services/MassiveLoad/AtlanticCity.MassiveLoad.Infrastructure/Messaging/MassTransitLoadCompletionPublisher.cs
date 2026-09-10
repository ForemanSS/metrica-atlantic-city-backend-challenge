using AtlanticCity.BuildingBlocks.Contracts.Notification;
using AtlanticCity.BuildingBlocks.Messaging;
using AtlanticCity.MassiveLoad.Application.Messaging;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace AtlanticCity.MassiveLoad.Infrastructure.Messaging;

internal sealed class MassTransitLoadCompletionPublisher(
    ISendEndpointProvider sendEndpointProvider,
    ILogger<MassTransitLoadCompletionPublisher> logger)
    : ILoadCompletionPublisher
{
    public async Task PublishAsync(
        LoadCompletionNotification notification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            notification);

        var endpoint =
            await sendEndpointProvider
                .GetSendEndpoint(
                    new Uri(
                        $"queue:{QueueNames.Notifications}?type=direct"));

        var message =
            new MassiveLoadCompleted(
                notification.LoadId,
                notification.UserEmail,
                notification.Result.ToString(),
                notification.TotalRows,
                notification.InsertedRows,
                notification.ExistingRows,
                notification.InvalidRows,
                notification.CorrelationId,
                notification.CompletedAt);

        await endpoint.Send(
            message,
            context =>
            {
                context.MessageId =
                    notification.LoadId;

                context.Headers.Set(
                    "X-Correlation-Id",
                    notification.CorrelationId);

                if (Guid.TryParse(
                        notification.CorrelationId,
                        out var correlationGuid))
                {
                    context.CorrelationId =
                        correlationGuid;
                }
            },
            cancellationToken);

        logger.LogInformation(
            "Massive load completion notification sent. LoadId {LoadId}, Queue {Queue}, Result {Result}",
            notification.LoadId,
            QueueNames.Notifications,
            notification.Result);
    }
}