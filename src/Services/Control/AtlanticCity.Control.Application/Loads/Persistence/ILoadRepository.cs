using AtlanticCity.Control.Domain.Loads;

namespace AtlanticCity.Control.Application.Loads.Persistence;

public interface ILoadRepository
{
    void Add(
        LoadFile load);

    void AddHistory(
        LoadStatusHistory history);

    Task<LoadFile?> FindByIdAsync(
        Guid loadId,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}