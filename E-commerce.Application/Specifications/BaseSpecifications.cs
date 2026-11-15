using E_commerce.Domain.Common;
using E_commerce.Domain.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Specifications
{
    public class BaseSpecifications<TEntity, TKey> :ISpecfications<TEntity,TKey> where TEntity : BaseEntity<TKey>
    {
        #region 1.Include
        public ICollection<Expression<Func<TEntity, object>>> IncludeExpressions { get; } = [];

        
        // Function used to add a related entity to the specification
        protected void AddInclude(Expression<Func<TEntity, object>> include)
        {
            IncludeExpressions.Add(include);
        }
        #endregion


        #region 2.Criteria (Condation)

        // constructor to initialize the specification with a criteria expression
        public BaseSpecifications(Expression<Func<TEntity, bool>>? criteria = null)
        {
            Criteria = criteria;
        }
        public Expression<Func<TEntity, bool>>? Criteria { get; }


        #endregion


        #region 3.Sorting
        public Expression<Func<TEntity, object>>? OrderBy { get; private set; }

        public Expression<Func<TEntity, object>>? OrderByDesc { get; private set; }

        protected void AddOrderBy(Expression<Func<TEntity, object>> orderByExpression)
        {
            OrderBy = orderByExpression;
        }

        protected void AddOrderByDesc(Expression<Func<TEntity, object>> orderByDescExpression)
        {
            OrderByDesc = orderByDescExpression;
        }
        #endregion

        #region 4.Pagination
        public int take { get; private set; }

        public int skip { get; private set; }

        public bool isPaginated { get; private set; }

        protected void ApplyPagination(int pageSize , int pageIndex)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
            ArgumentOutOfRangeException.ThrowIfLessThan(pageIndex, 1);

            var skip = (long)(pageIndex - 1) * pageSize;
            if (skip > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(pageIndex), "The requested page exceeds the supported range.");

            isPaginated = true;
            take = pageSize;
            skip = (int)skip;
        }
        #endregion
    }
}
