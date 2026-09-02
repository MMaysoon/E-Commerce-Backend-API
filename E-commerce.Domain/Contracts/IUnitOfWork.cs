using E_commerce.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Domain.Contracts
{
    public interface IUnitOfWork
    {
        IGenericRepository<TEntity, TKey> GetRepository <TEntity, TKey>() where TEntity : BaseEntity<TKey>;

        Task<int> SaveChangesAsync(CancellationToken ct=default);
    }
}
