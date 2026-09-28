using E_commerce.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Domain.Contracts
{
    public interface IGenericRepository<TEntity,TKey> where TEntity : BaseEntity<TKey>
    {

        Task<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default);

        // Get by Id using Specification DP
        Task<TEntity?> GetByIdAsync(ISpecfications<TEntity,TKey> spec, CancellationToken ct = default);
        Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken ct=default);

        // Get All using Specification DP
        Task<IEnumerable<TEntity>> GetAllAsync(ISpecfications<TEntity,TKey> spec,CancellationToken ct = default);

        void Add(TEntity entity);

        void Update(TEntity entity);

        void Delete(TEntity entity);
    }
}
