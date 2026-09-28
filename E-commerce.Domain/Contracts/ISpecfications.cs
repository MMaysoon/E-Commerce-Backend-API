using E_commerce.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Domain.Contracts
{
    public interface ISpecfications<TEntity,TKey> where TEntity : BaseEntity<TKey>
    {
        //1.Include
        ICollection<Expression<Func<TEntity,object>>> IncludeExpressions { get; }

        //2.Criteria
        Expression<Func<TEntity,bool>>? Criteria { get; }

        //3.Sorting 
        Expression<Func<TEntity,object>>? OrderBy { get; }
        Expression<Func<TEntity,object>>? OrderByDesc { get; }
    }
}
