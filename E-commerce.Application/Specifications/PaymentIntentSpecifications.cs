using E_commerce.Domain.Entities.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Specifications
{
    public class PaymentIntentSpecifications:BaseSpecifications<Order,Guid>
    {
        public PaymentIntentSpecifications (string PaymentIntentId)
            :base(o => o.PaymentIntentId == PaymentIntentId)
        {
            AddInclude(o => o.Items);
            AddInclude(o => o.DeliveryMethod);
            AddInclude(o => o.ShipToAddress);
        }
    }
}
