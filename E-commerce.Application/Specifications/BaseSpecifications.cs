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
        public BaseSpecifications(Expression<Func<TEntity, bool>> criteria)
        {
            Criteria = criteria;
        }
        public Expression<Func<TEntity, bool>> Criteria { get; private set; }
        

        #endregion
    }
}
