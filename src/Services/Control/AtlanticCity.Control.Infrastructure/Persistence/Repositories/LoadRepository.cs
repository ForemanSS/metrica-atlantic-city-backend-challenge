using AtlanticCity.Control.Application.Loads.Persistence;
using AtlanticCity.Control.Domain.Loads;
using Microsoft.EntityFrameworkCore;

namespace AtlanticCity.Control.Infrastructure.Persistence.Repositories;

internal sealed class LoadRepository(
    ControlDbContext dbContext)
    : ILoadRepository
{
    public void Add(
        LoadFile load)
    {
        dbContext.LoadFiles.Add(
            load);
    }

    public void AddHistory(
        LoadStatusHistory history)
    {
        dbContext.LoadStatusHistory.Add(
            history);
    }

    public Task<LoadFile?> FindByIdAsync(
        Guid loadId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.LoadFiles
            .SingleOrDefaultAsync(
                x => x.Id == loadId,
                cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(
            cancellationToken);
    }
}