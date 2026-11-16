
using E_commerce.Domain.Contracts;
using StackExchange.Redis;

namespace E_commerce.Infrastructure.Repositories
{
    public class CacheRepository : ICacheRepository
    {
        private readonly IDatabase _database;

        public CacheRepository(IConnectionMultiplexer connection)
        {
            _database = connection.GetDatabase();
        }

        public async Task<string?> GetAsync( string cacheKey,CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var value = await _database.StringGetAsync(cacheKey);

            return value.IsNullOrEmpty ? null : value.ToString();
        }

        public async Task SetAsync(string cacheKey,string cacheValue, TimeSpan timeToLive, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _database.StringSetAsync( cacheKey, cacheValue,  timeToLive);
        }

        public async Task<long> GetGroupVersionAsync( string groupName,CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var versionKey = $"cache-version:{groupName}";

            var version = await _database.StringGetAsync(versionKey);

            if (version.IsNullOrEmpty)
                return 0;

            return (long)version;
        }

        public async Task InvalidateGroupAsync(string groupName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var versionKey = $"cache-version:{groupName}";

            await _database.StringIncrementAsync(versionKey);
        }
    }
}
