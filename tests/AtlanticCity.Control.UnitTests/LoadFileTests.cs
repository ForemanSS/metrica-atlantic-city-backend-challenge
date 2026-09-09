using AtlanticCity.Control.Domain.Loads;

namespace AtlanticCity.Control.UnitTests;

public sealed class LoadFileTests
{
    [Fact]
    public void Create_ShouldCreatePendingLoad()
    {
        var load = LoadFile.Create(
            "productos.xlsx",
            "user-1",
            "user@atlanticcity.pe",
            "correlation-123");

        Assert.Equal(LoadStatus.Pending, load.Status);
        Assert.Equal(LoadResult.Pending, load.Result);
        Assert.Equal("productos.xlsx", load.FileName);
    }

    [Fact]
    public void NormalFlow_ShouldRespectExpectedStateTransitions()
    {
        var load = LoadFile.Create(
            "productos.xlsx",
            "user-1",
            "user@atlanticcity.pe",
            "correlation-123");

        load.SetStoragePath("/loads/file.xlsx");

        load.MarkProcessing("202609");

        load.MarkLoaded(
            totalRows: 100,
            validRows: 95,
            insertedRows: 90,
            existingRows: 5,
            invalidRows: 5);

        load.MarkCompleted(LoadResult.Partial);

        load.MarkNotified();

        Assert.Equal(LoadStatus.Notified, load.Status);
        Assert.Equal(LoadResult.Partial, load.Result);
    }

    [Fact]
    public void MarkNotified_FromPending_ShouldThrow()
    {
        var load = LoadFile.Create(
            "productos.xlsx",
            "user-1",
            "user@atlanticcity.pe",
            "correlation-123");

        Assert.Throws<InvalidOperationException>(
            () => load.MarkNotified());
    }

    [Fact]
    public void Reject_ShouldFinishLoadAsRejected()
    {
        var load = LoadFile.Create(
            "productos.xlsx",
            "user-1",
            "user@atlanticcity.pe",
            "correlation-123");

        load.MarkProcessing("202609");

        load.Reject("Period already processed.");

        Assert.Equal(LoadStatus.Completed, load.Status);
        Assert.Equal(LoadResult.Rejected, load.Result);
        Assert.NotNull(load.CompletedAt);
    }
}