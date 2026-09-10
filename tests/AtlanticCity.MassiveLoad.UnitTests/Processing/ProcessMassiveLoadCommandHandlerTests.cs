using AtlanticCity.MassiveLoad.Application.Excel;
using AtlanticCity.MassiveLoad.Application.Persistence;
using AtlanticCity.MassiveLoad.Application.Processing;
using AtlanticCity.MassiveLoad.Application.Storage;
using AtlanticCity.MassiveLoad.Domain.Rows;

namespace AtlanticCity.MassiveLoad.UnitTests.Processing;

public sealed class ProcessMassiveLoadCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenLoadIsValid_ShouldComplete()
    {
        var loadId =
            Guid.NewGuid();

        var store =
            new FakeStore
            {
                Snapshot =
                    CreatePendingSnapshot(
                        loadId),

                Reservation =
                    new PeriodReservationResult(
                        PeriodReservationDecision.Reserved),

                PersistenceResult =
                    new LoadPersistenceResult(
                        1,
                        1,
                        1,
                        0,
                        0,
                        LoadProcessingResult.Success)
            };

        var rows =
            new[]
            {
                new RawProductRow(
                    2,
                    "2026-09",
                    "PROD-001",
                    "Product",
                    "Description",
                    "Category",
                    "1",
                    "10.50")
            };

        var handler =
            CreateHandler(
                store,
                rows);

        var result =
            await handler.HandleAsync(
                CreateCommand(loadId));

        Assert.Equal(
            LoadProcessingState.Completed,
            result.Status);

        Assert.Equal(
            LoadProcessingResult.Success,
            result.Result);

        Assert.Equal(
            1,
            result.InsertedRows);

        Assert.True(
            store.ProcessingHistoryAdded);

        Assert.NotNull(
            store.CompletedRequest);
    }

    [Fact]
    public async Task Handle_WhenExcelHasMultiplePeriods_ShouldReject()
    {
        var loadId =
            Guid.NewGuid();

        var store =
            new FakeStore
            {
                Snapshot =
                    CreatePendingSnapshot(
                        loadId)
            };

        var rows =
            new[]
            {
                CreateRow(
                    2,
                    "2026-09",
                    "PROD-001"),

                CreateRow(
                    3,
                    "2026-10",
                    "PROD-002")
            };

        var handler =
            CreateHandler(
                store,
                rows);

        var result =
            await handler.HandleAsync(
                CreateCommand(loadId));

        Assert.Equal(
            LoadProcessingResult.Rejected,
            result.Result);

        Assert.Equal(
            "MULTIPLE_PERIODS",
            store.RejectedErrorCode);

        Assert.Null(
            store.CompletedRequest);
    }

    [Fact]
    public async Task Handle_WhenPeriodWasAlreadyProcessed_ShouldReject()
    {
        var loadId =
            Guid.NewGuid();

        var store =
            new FakeStore
            {
                Snapshot =
                    CreatePendingSnapshot(
                        loadId),

                Reservation =
                    new PeriodReservationResult(
                        PeriodReservationDecision.Rejected,
                        Guid.NewGuid(),
                        LoadProcessingState.Completed)
            };

        var handler =
            CreateHandler(
                store,
                [
                    CreateRow(
                        2,
                        "2026-09",
                        "PROD-001")
                ]);

        var result =
            await handler.HandleAsync(
                CreateCommand(loadId));

        Assert.Equal(
            LoadProcessingResult.Rejected,
            result.Result);

        Assert.Equal(
            "PERIOD_ALREADY_PROCESSED",
            store.RejectedErrorCode);

        Assert.Null(
            store.CompletedRequest);
    }

    [Fact]
    public async Task Handle_WhenLoadIsAlreadyCompleted_ShouldSkip()
    {
        var loadId =
            Guid.NewGuid();

        var store =
            new FakeStore
            {
                Snapshot =
                    new LoadSnapshot(
                        loadId,
                        LoadProcessingState.Completed,
                        LoadProcessingResult.Success,
                        "2026-09",
                        "admin@atlanticcity.pe",
                        "correlation")
            };

        var fileSource =
            new FakeFileSource();

        var handler =
            new ProcessMassiveLoadCommandHandler(
                fileSource,
                new FakeExcelReader([]),
                store);

        var result =
            await handler.HandleAsync(
                CreateCommand(loadId));

        Assert.True(
            result.Skipped);

        Assert.False(
            fileSource.WasOpened);
    }

    private static ProcessMassiveLoadCommandHandler CreateHandler(
        FakeStore store,
        IReadOnlyCollection<RawProductRow> rows)
    {
        return new ProcessMassiveLoadCommandHandler(
            new FakeFileSource(),
            new FakeExcelReader(rows),
            store);
    }

    private static ProcessMassiveLoadCommand CreateCommand(
        Guid loadId)
    {
        return new ProcessMassiveLoadCommand(
            loadId,
            "seaweed://filer/loads/file.xlsx",
            "file.xlsx",
            "user-1",
            "admin@atlanticcity.pe",
            "correlation",
            DateTimeOffset.UtcNow);
    }

    private static LoadSnapshot CreatePendingSnapshot(
        Guid loadId)
    {
        return new LoadSnapshot(
            loadId,
            LoadProcessingState.Pending,
            LoadProcessingResult.Pending,
            null,
            "admin@atlanticcity.pe",
            "correlation");
    }

    private static RawProductRow CreateRow(
        int rowNumber,
        string period,
        string productCode)
    {
        return new RawProductRow(
            rowNumber,
            period,
            productCode,
            "Product",
            "Description",
            "Category",
            "1",
            "10.50");
    }

    private sealed class FakeFileSource
        : ILoadFileSource
    {
        public bool WasOpened { get; private set; }

        public Task<Stream> OpenReadAsync(
            string storagePath,
            CancellationToken cancellationToken = default)
        {
            WasOpened = true;

            Stream stream =
                new MemoryStream(
                    [1, 2, 3]);

            return Task.FromResult(
                stream);
        }
    }

    private sealed class FakeExcelReader(
        IReadOnlyCollection<RawProductRow> rows)
        : IExcelLoadReader
    {
        public Task<IReadOnlyCollection<RawProductRow>> ReadAsync(
            Stream content,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                rows);
        }
    }

    private sealed class FakeStore
        : IMassiveLoadStore
    {
        public LoadSnapshot? Snapshot { get; init; }

        public PeriodReservationResult Reservation { get; init; } =
            new(
                PeriodReservationDecision.Reserved);

        public LoadPersistenceResult PersistenceResult { get; init; } =
            new(
                0,
                0,
                0,
                0,
                0,
                LoadProcessingResult.Success);

        public bool ProcessingHistoryAdded { get; private set; }

        public string? RejectedErrorCode { get; private set; }

        public LoadPersistenceRequest? CompletedRequest { get; private set; }

        public Task<LoadSnapshot?> FindLoadAsync(
            Guid loadId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                Snapshot);
        }

        public Task<PeriodReservationResult> TryReservePeriodAsync(
            Guid loadId,
            string period,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                Reservation);
        }

        public Task AddProcessingHistoryAsync(
            Guid loadId,
            string correlationId,
            CancellationToken cancellationToken = default)
        {
            ProcessingHistoryAdded = true;

            return Task.CompletedTask;
        }

        public Task<LoadPersistenceResult> CompleteAsync(
            LoadPersistenceRequest request,
            CancellationToken cancellationToken = default)
        {
            CompletedRequest =
                request;

            return Task.FromResult(
                PersistenceResult);
        }

        public Task RejectAsync(
            Guid loadId,
            string correlationId,
            string errorCode,
            string message,
            CancellationToken cancellationToken = default)
        {
            RejectedErrorCode =
                errorCode;

            return Task.CompletedTask;
        }

        public Task FailAsync(
            Guid loadId,
            string correlationId,
            string errorCode,
            string message,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}