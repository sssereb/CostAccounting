// PortfolioApp.Application/Extensions/AssetRepositoryExtensions.cs

using PortfolioApp.Application.Repositories.Interfaces;
using PortfolioApp.Domain;


public static class AssetRepositoryExtensions
{
    /// <summary>
    /// Возвращает Asset по тикеру или создаёт, если нет.
    /// </summary>
    
    public static Asset GetOrCreate(this IAssetRepository repo, string ticker) =>
        repo.GetByTicker(ticker) ?? repo.Create(ticker);

}