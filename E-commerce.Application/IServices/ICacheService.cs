
namespace E_commerce.Application.IServices
{
    public interface ICacheService
    {
        Task<string?> GetAsync(string cacheKey,CancellationToken cancellationToken = default);

        Task SetAsync( string cacheKey,object cacheValue, TimeSpan timeToLive, CancellationToken cancellationToken = default);

        Task<long> GetGroupVersionAsync(string groupName,  CancellationToken cancellationToken = default);

        Task InvalidateGroupAsync( string groupName, CancellationToken cancellationToken = default);
    }
}
