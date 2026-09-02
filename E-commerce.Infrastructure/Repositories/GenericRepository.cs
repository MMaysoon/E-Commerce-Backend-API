using E_commerce.Domain.Common;
using E_commerce.Domain.Contracts;
using E_commerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Infrastructure.Repositories
{
    public class GenericRepository<TEntity, TKey>(StoreDBContext dBContext) : IGenericRepository<TEntity, TKey> where TEntity : BaseEntity<TKey>
    {
        public async Task<IEnumerable<TEntity>> GetAllAsync()
          => await dBContext.Set<TEntity>().ToListAsync();

        public async Task<TEntity?> GetByIdAsync(TKey id)
            => await dBContext.Set<TEntity>().FindAsync(id);

        public void Add(TEntity entity)
            => dBContext.Set<TEntity>().Add(entity);

        public void Delete(TEntity entity)
           => dBContext.Set<TEntity>().Remove(entity);

        public void Update(TEntity entity)
           => dBContext.Set<TEntity>().Update(entity);
    }
}
