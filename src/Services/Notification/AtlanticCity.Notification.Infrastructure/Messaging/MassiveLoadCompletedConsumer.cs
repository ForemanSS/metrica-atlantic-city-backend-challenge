using AtlanticCity.BuildingBlocks.Contracts.Notification;
using AtlanticCity.Notification.Application.Notifications;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace AtlanticCity.Notification.Infrastructure.Messaging;

internal sealed class MassiveLoadCompletedConsumer(
    SendLoadNotificationCommandHandler handler,
    ILogger<MassiveLoadCompletedConsumer> logger)
    : IConsumer<MassiveLoadCompleted>
{
    public async Task Consume(
        ConsumeContext<MassiveLoadCompleted> context)
    {
        var message =
            context.Message;

        using var logScope =
            logger.BeginScope(
                new Dictionary<string, object?>
                {
                    ["CorrelationId"] =
                        message.CorrelationId,

                    ["LoadId"] =
                        message.LoadId
                });

        logger.LogInformation(
            "Notification message received. Result {Result}.",
            message.Result);

        var result =
            await handler.HandleAsync(
                new SendLoadNotificationCommand(
                    message.LoadId,
                    message.UserEmail,
                    message.Result,
                    message.TotalRows,
                    message.InsertedRows,
                    message.ExistingRows,
                    message.InvalidRows,
                    message.CorrelationId,
                    message.CompletedAt),
                context.CancellationToken);

        logger.LogInformation(
            "Notification processing completed. LoadId {LoadId}, Sent {Sent}, AlreadyNotified {AlreadyNotified}",
            result.LoadId,
            result.Sent,
            result.AlreadyNotified);
    }
}