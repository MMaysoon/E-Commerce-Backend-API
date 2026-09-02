using E_commerce.Domain.Common;
using E_commerce.Domain.Contracts;
using E_commerce.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Infrastructure.Repositories
{
    public class UnitOfWork(StoreDBContext dBContext) : IUnitOfWork
    {

        private readonly Dictionary<string, object> repositories = new();
        //1.Get Repository using -> IGenericRepository
        public IGenericRepository<TEntity, TKey> GetRepository <TEntity, TKey>() where TEntity : BaseEntity<TKey>
        {
            var typeName = typeof(TEntity).Name;

            if (repositories.TryGetValue(typeName,out object? value))
                return (IGenericRepository<TEntity, TKey>)value;

            var repo = new GenericRepository<TEntity, TKey>(dBContext);
            repositories[typeName] = repo;
            return repo;
        }

        //2.Save Changes
        public Task<int> SaveChangesAsync(CancellationToken ct=default)
            => dBContext.SaveChangesAsync(ct);
    }
}