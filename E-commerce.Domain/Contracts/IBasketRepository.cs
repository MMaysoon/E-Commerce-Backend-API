using E_commerce.Domain.Entities.Baskets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Domain.Contracts
{
    public interface IBasketRepository
    {
        Task<CustomerBasket?> GetBasketAsync (string basketId,CancellationToken ct=default);
        Task<CustomerBasket?> CreateOrUpdateAsync(CustomerBasket basket,TimeSpan? timeToLive=null,CancellationToken ct=default);

        Task<bool> DeleteBasketAsync (string basketId,CancellationToken ct=default);
    }
}
