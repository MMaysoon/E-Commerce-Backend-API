using E_commerce.Application.Specifications;
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
        public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken ct = default)
          => await dBContext.Set<TEntity>().ToListAsync(ct);

        public async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default)
            => await dBContext.Set<TEntity>().FindAsync(id,ct);

        public void Add(TEntity entity)
            => dBContext.Set<TEntity>().Add(entity);

        public void Delete(TEntity entity)
           => dBContext.Set<TEntity>().Remove(entity);

        public void Update(TEntity entity)
           => dBContext.Set<TEntity>().Update(entity);


        // Using Specification DP
        public async Task<IEnumerable<TEntity>> GetAllAsync(ISpecfications<TEntity, TKey>? spec, CancellationToken ct = default)
        {
            #region 1'st Way 
            // 1. Get the DbSet for the entity type
            //IQueryable<TEntity> query = dBContext.Set<TEntity>();

            //// 2. Apply Include expressions from the specification
            //if (spec != null)
            //{
            //    foreach (var expression in spec.IncludeExpressions)
            //    {
            //        // Apply the Include expression to the query
            //        query = query.Include(expression);
            //    }
            //}

            //// 3. Execute the query
            //return await query.ToListAsync(ct);
            #endregion

            #region 2'nd Way (using SpecificationEvaluator)
            return await SpecficationEvaluator.CreateQuery(dBContext.Set<TEntity>(), spec).ToListAsync(ct);
            #endregion
        }

        // Get by Id using Specification DP
        public async Task<TEntity?> GetByIdAsync(ISpecfications<TEntity, TKey> spec, CancellationToken ct = default)
            => await SpecficationEvaluator.CreateQuery(dBContext.Set<TEntity>(), spec).FirstOrDefaultAsync(ct);
    }
}
