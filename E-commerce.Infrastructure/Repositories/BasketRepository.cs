using E_commerce.Domain.Contracts;
using E_commerce.Domain.Entities.Baskets;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace E_commerce.Infrastructure.Repositories
{
    public class BasketRepository : IBasketRepository
    {
        public readonly IDatabase _database;
        public BasketRepository(IConnectionMultiplexer connection)
        {
            _database = connection.GetDatabase();
        }
        public async Task<CustomerBasket?> CreateOrUpdateAsync(CustomerBasket basket, TimeSpan? timeToLive=null, CancellationToken ct = default)
        {
            var json = JsonSerializer.Serialize(basket);
            // Redis key -> Basket Id
            var success = await _database.StringSetAsync(basket.Id, json, timeToLive ?? TimeSpan.FromDays(30));
            return success ? basket : null;
        }

        public async Task<bool> DeleteBasketAsync(string basketId, CancellationToken ct = default)
        {
            return await _database.KeyDeleteAsync(basketId);
        }

        public async Task<CustomerBasket?> GetBasketAsync(string basketId, CancellationToken ct = default)
        {
           var basket =  await _database.StringGetAsync(basketId);
            
           return basket.IsNullOrEmpty? null : JsonSerializer.Deserialize<CustomerBasket>(basket!);

        }
    }
}
