using E_commerce.Domain.Common;
using E_commerce.Domain.Contracts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Specifications
{
    // convert the specification into a queryable object using (EF core )that can be executed against a data source
    public static class SpecficationEvaluator
    {
        public static IQueryable<TEntity> CreateQuery <TEntity,TKey>(IQueryable<TEntity> inputQuery, ISpecfications<TEntity, TKey> spec) where TEntity :BaseEntity<TKey>
        {
            ArgumentNullException.ThrowIfNull(inputQuery);
            ArgumentNullException.ThrowIfNull(spec);

            var query = inputQuery;

            //1.Criteria
            if (spec.Criteria is  not null)
            {
                query = query.Where(spec.Criteria);
            }

            //2.Include 
            if (spec.IncludeExpressions.Any())
                query=spec.IncludeExpressions.Aggregate(query, (current, include) => current.Include(include));


            //3.Sorting 
            if (spec.OrderBy is not null)
            {
                query = query.OrderBy(spec.OrderBy);
            }

            if (spec.OrderByDesc is not null)
            {
                query = query.OrderByDescending(spec.OrderByDesc);
            }


            //4.Pagination
            if (spec.isPaginated)
            {
                ArgumentOutOfRangeException.ThrowIfNegative(spec.skip);
                ArgumentOutOfRangeException.ThrowIfLessThan(spec.take, 1);
                query = query.Skip(spec.skip).Take(spec.take);
            }
            return query; 
        }
    }
}
