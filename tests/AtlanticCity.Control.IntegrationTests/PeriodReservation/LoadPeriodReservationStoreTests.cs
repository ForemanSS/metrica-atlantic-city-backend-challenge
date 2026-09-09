using AtlanticCity.Control.Application.Loads.PeriodReservation;
using AtlanticCity.Control.Domain.Loads;
using AtlanticCity.Control.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace AtlanticCity.Control.IntegrationTests.PeriodReservation;

[Collection(PostgreSqlCollection.Name)]
public sealed class LoadPeriodReservationStoreTests
{
    private readonly PostgreSqlFixture _fixture;

    public LoadPeriodReservationStoreTests(
        PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task TryReserveAsync_WhenPeriodIsAvailable_ShouldReserveIt()
    {
        var load = await CreatePendingLoadAsync();

        await using var provider =
            TestServiceProviderFactory.Create(
                _fixture.ConnectionString);

        var store =
            provider.GetRequiredService<
                ILoadPeriodReservationStore>();

        var result =
            await store.TryReserveAsync(
                load.Id,
                CreatePeriod());

        Assert.Equal(
            PeriodReservationDecision.Reserved,
            result.Decision);

        await using var dbContext =
            _fixture.CreateDbContext();

        var persisted =
            await dbContext.LoadFiles
                .SingleAsync(x => x.Id == load.Id);

        Assert.Equal(
            LoadStatus.Processing,
            persisted.Status);

        Assert.NotNull(
            persisted.ProcessingStartedAt);
    }

    [Fact]
    public async Task TryReserveAsync_WhenSameLoadIsRetried_ShouldBeIdempotent()
    {
        var load = await CreatePendingLoadAsync();

        var period = CreatePeriod();

        await using var provider =
            TestServiceProviderFactory.Create(
                _fixture.ConnectionString);

        var store =
            provider.GetRequiredService<
                ILoadPeriodReservationStore>();

        var first =
            await store.TryReserveAsync(
                load.Id,
                period);

        var second =
            await store.TryReserveAsync(
                load.Id,
                period);

        Assert.Equal(
            PeriodReservationDecision.Reserved,
            first.Decision);

        Assert.Equal(
            PeriodReservationDecision.AlreadyReserved,
            second.Decision);
    }

    [Fact]
    public async Task TryReserveAsync_WhenPeriodIsProcessing_ShouldBlockSecondLoad()
    {
        var firstLoad =
            await CreatePendingLoadAsync();

        var secondLoad =
            await CreatePendingLoadAsync();

        var period = CreatePeriod();

        await using var provider =
            TestServiceProviderFactory.Create(
                _fixture.ConnectionString);

        var store =
            provider.GetRequiredService<
                ILoadPeriodReservationStore>();

        await store.TryReserveAsync(
            firstLoad.Id,
            period);

        var result =
            await store.TryReserveAsync(
                secondLoad.Id,
                period);

        Assert.Equal(
            PeriodReservationDecision.Blocked,
            result.Decision);

        Assert.Equal(
            firstLoad.Id,
            result.ConflictingLoadId);

        Assert.Equal(
            LoadStatus.Processing,
            result.ConflictingStatus);
    }

    [Fact]
    public async Task TryReserveAsync_WhenPeriodWasLoaded_ShouldRejectNewLoad()
    {
        var firstLoad =
            await CreatePendingLoadAsync();

        var secondLoad =
            await CreatePendingLoadAsync();

        var period = CreatePeriod();

        await using var provider =
            TestServiceProviderFactory.Create(
                _fixture.ConnectionString);

        var store =
            provider.GetRequiredService<
                ILoadPeriodReservationStore>();

        await store.TryReserveAsync(
            firstLoad.Id,
            period);

        await using (var dbContext =
                     _fixture.CreateDbContext())
        {
            var persisted =
                await dbContext.LoadFiles
                    .SingleAsync(
                        x => x.Id == firstLoad.Id);

            persisted.MarkLoaded(
                totalRows: 1,
                validRows: 1,
                insertedRows: 1,
                existingRows: 0,
                invalidRows: 0);

            await dbContext.SaveChangesAsync();
        }

        var result =
            await store.TryReserveAsync(
                secondLoad.Id,
                period);

        Assert.Equal(
            PeriodReservationDecision.Rejected,
            result.Decision);

        Assert.Equal(
            firstLoad.Id,
            result.ConflictingLoadId);

        Assert.Equal(
            LoadStatus.Loaded,
            result.ConflictingStatus);
    }

    [Fact]
    public async Task TryReserveAsync_WhenTwoLoadsRaceForSamePeriod_ShouldAllowOnlyOne()
    {
        var firstLoad =
            await CreatePendingLoadAsync();

        var secondLoad =
            await CreatePendingLoadAsync();

        var period = CreatePeriod();

        await using var provider =
            TestServiceProviderFactory.Create(
                _fixture.ConnectionString);

        var store =
            provider.GetRequiredService<
                ILoadPeriodReservationStore>();

        var firstTask =
            store.TryReserveAsync(
                firstLoad.Id,
                period);

        var secondTask =
            store.TryReserveAsync(
                secondLoad.Id,
                period);

        var results =
            await Task.WhenAll(
                firstTask,
                secondTask);

        Assert.Single(
            results,
            x => x.Decision ==
                 PeriodReservationDecision.Reserved);

        Assert.Single(
            results,
            x => x.Decision ==
                 PeriodReservationDecision.Blocked);
    }

    [Fact]
    public async Task DatabaseConstraint_WhenReservationProcedureIsBypassed_ShouldRejectDuplicateActivePeriod()
    {
        var firstLoad =
            await CreatePendingLoadAsync();

        var secondLoad =
            await CreatePendingLoadAsync();

        var period = CreatePeriod();

        await using var provider =
            TestServiceProviderFactory.Create(
                _fixture.ConnectionString);

        var store =
            provider.GetRequiredService<
                ILoadPeriodReservationStore>();

        var reservation =
            await store.TryReserveAsync(
                firstLoad.Id,
                period);

        Assert.Equal(
            PeriodReservationDecision.Reserved,
            reservation.Decision);

        await using var dbContext =
            _fixture.CreateDbContext();

        var exception =
            await Assert.ThrowsAsync<PostgresException>(
                async () =>
                {
                    await dbContext.Database.ExecuteSqlInterpolatedAsync(
                        $"""
                    UPDATE carga_archivo
                    SET
                        periodo = {period},
                        estado = 'Processing',
                        fecha_inicio_proceso = CURRENT_TIMESTAMP
                    WHERE id = {secondLoad.Id};
                    """);
                });

        Assert.Equal(
            PostgresErrorCodes.UniqueViolation,
            exception.SqlState);

        Assert.Equal(
            "ux_carga_archivo_periodo_bloqueante",
            exception.ConstraintName);
    }

    private async Task<LoadFile> CreatePendingLoadAsync()
    {
        var load =
            LoadFile.Create(
                $"products-{Guid.NewGuid():N}.xlsx",
                "integration-test-user",
                "integration@atlanticcity.pe",
                Guid.NewGuid().ToString("N"));

        await using var dbContext =
            _fixture.CreateDbContext();

        dbContext.LoadFiles.Add(load);

        await dbContext.SaveChangesAsync();

        return load;
    }

    private static string CreatePeriod()
    {
        return $"T-{Guid.NewGuid():N}"[..20];
    }
}