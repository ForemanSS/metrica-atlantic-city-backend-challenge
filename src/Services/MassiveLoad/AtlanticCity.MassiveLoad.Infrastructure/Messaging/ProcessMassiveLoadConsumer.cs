using AtlanticCity.BuildingBlocks.Contracts.MassiveLoad;
using AtlanticCity.MassiveLoad.Application.Messaging;
using AtlanticCity.MassiveLoad.Application.Persistence;
using AtlanticCity.MassiveLoad.Application.Processing;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace AtlanticCity.MassiveLoad.Infrastructure.Messaging;

internal sealed class ProcessMassiveLoadConsumer(
    ProcessMassiveLoadCommandHandler handler,
    ILoadCompletionPublisher completionPublisher,
    ILogger<ProcessMassiveLoadConsumer> logger)
    : IConsumer<ProcessMassiveLoad>
{
    public async Task Consume(
        ConsumeContext<ProcessMassiveLoad> context)
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
            "Massive load message received.");

        var command =
            new ProcessMassiveLoadCommand(
                message.LoadId,
                message.StoragePath,
                message.FileName,
                message.UserId,
                message.UserEmail,
                message.CorrelationId,
                message.RequestedAt);

        var result =
            await handler.HandleAsync(
                command,
                context.CancellationToken);

        if (result.Status ==
            LoadProcessingState.Notified)
        {
            logger.LogInformation(
                "Massive load already notified. LoadId {LoadId}. Message will be acknowledged.",
                result.LoadId);

            return;
        }

        if (result.Status !=
            LoadProcessingState.Completed)
        {
            throw new InvalidOperationException(
                $"Load '{result.LoadId}' finished message processing in unexpected status '{result.Status}'.");
        }

        await completionPublisher.PublishAsync(
            new LoadCompletionNotification(
                result.LoadId,
                message.UserEmail,
                result.Result,
                result.TotalRows,
                result.InsertedRows,
                result.ExistingRows,
                result.InvalidRows,
                message.CorrelationId,
                DateTimeOffset.UtcNow),
            context.CancellationToken);

        logger.LogInformation(
            "Massive load message completed. LoadId {LoadId}, Result {Result}, Skipped {Skipped}",
            result.LoadId,
            result.Result,
            result.Skipped);
    }
}