
using E_commerce.Application.IServices;
using E_commerce.Domain.Contracts;
using System.Text.Json;

namespace E_commerce.Application.Services
{
    public class CacheService : ICacheService
    {
        private readonly ICacheRepository _cacheRepository;

        public CacheService(ICacheRepository cacheRepository)
        {
            _cacheRepository = cacheRepository;
        }

        public Task<string?> GetAsync(string cacheKey,CancellationToken cancellationToken = default)
            => _cacheRepository.GetAsync(cacheKey, cancellationToken);

        public Task SetAsync( string cacheKey, object cacheValue,TimeSpan timeToLive, CancellationToken cancellationToken = default)
        {
            var json = JsonSerializer.Serialize(
                cacheValue,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

            return _cacheRepository.SetAsync( cacheKey, json, timeToLive, cancellationToken);
        }

        public Task<long> GetGroupVersionAsync( string groupName, CancellationToken cancellationToken = default)
            => _cacheRepository.GetGroupVersionAsync( groupName, cancellationToken);

        public Task InvalidateGroupAsync( string groupName, CancellationToken cancellationToken = default)
            => _cacheRepository.InvalidateGroupAsync( groupName,  cancellationToken);
    }
}
