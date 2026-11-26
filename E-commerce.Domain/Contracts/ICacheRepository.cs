
namespace E_commerce.Domain.Contracts
{
    public interface ICacheRepository
    {
        Task<string?> GetAsync(string cacheKey, CancellationToken cancellationToken = default);

        Task SetAsync( string cacheKey,string cacheValue, TimeSpan timeToLive, CancellationToken cancellationToken = default);

        Task<long> GetGroupVersionAsync(string groupName, CancellationToken cancellationToken = default);

        Task InvalidateGroupAsync( string groupName, CancellationToken cancellationToken = default);
    }
}
