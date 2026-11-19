using E_commerce.Domain.Entities.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Specifications
{
    public class OrderSpecfications :BaseSpecifications<Order,Guid>
    {
        public OrderSpecfications(string email):base(o=>o.BuyerEmail==email)
        {
            AddInclude(o => o.DeliveryMethod);
            AddInclude(o=>o.Items);
            AddOrderByDesc(o => o.DateTimeOffset); //Order Date
        }

        public OrderSpecfications(Guid id ,string email):base (o=>o.Id==id &&o.BuyerEmail==email) 
        {
            AddInclude(o => o.DeliveryMethod);
            AddInclude(o=>o.Items);
        }
    }
}
