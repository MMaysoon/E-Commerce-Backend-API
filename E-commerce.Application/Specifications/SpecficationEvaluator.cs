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
    public static class SpecficationEvaluator
    {
        public static IQueryable<TEntity> CreateQuery <TEntity,TKey>(IQueryable<TEntity> inputQuery, ISpecfications<TEntity, TKey> spec) where TEntity :BaseEntity<TKey>
        {
            var query = inputQuery;
           
            //1.Include 
            if (spec.IncludeExpressions.Any())
                query=spec.IncludeExpressions.Aggregate(query, (current, include) => current.Include(include));

            return query; 
        }
    }
}
