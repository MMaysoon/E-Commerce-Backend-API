using E_commerce.Domain.Entities.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Specifications
{
    public class OrderTrackingSpecifications : BaseSpecifications<Order, Guid>
    {
        public OrderTrackingSpecifications(Guid orderId)
            : base(o => o.Id == orderId)
        {
            AddInclude(o => o.Items);
            AddInclude(o => o.DeliveryMethod);
            AddInclude(o => o.ShipToAddress);
        }
    }
}
