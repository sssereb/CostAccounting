using PortfolioApp.Application.Repositories.Interfaces;
using PortfolioApp.Domain;

namespace PortfolioApp.Tests.Support;

/// <summary>Pauses right after lots are read, so a test can interleave another sale deterministically.</summary>
internal sealed class PausingLotRepository : ILotRepository
{
    private readonly ILotRepository _inner;
    private readonly TaskCompletionSource _read = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource _resume = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public PausingLotRepository(ILotRepository inner) => _inner = inner;

    public Task LotsRead => _read.Task;

    public void Resume() => _resume.SetResult();

    public async Task<IReadOnlyList<Lot>> GetForAssetAsync(Guid assetId, CancellationToken ct = default)
    {
        var lots = await _inner.GetForAssetAsync(assetId, ct);
        _read.SetResult();
        await _resume.Task;
        return lots;
    }

    public Task SaveAsync(Lot lot, CancellationToken ct = default) => _inner.SaveAsync(lot, ct);

    public Task<IReadOnlyList<Lot>> GetAllAsync(CancellationToken ct = default) => _inner.GetAllAsync(ct);
}
