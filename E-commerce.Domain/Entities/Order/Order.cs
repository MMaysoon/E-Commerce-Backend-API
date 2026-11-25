using E_commerce.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Domain.Entities.Order
{
    public class Order:BaseEntity<Guid>
    {
        public Order()
        {
            
        }

        public Order(string buyerEmail, ICollection<OrderItem> items, OrderAddress shipToAddress, DeliveryMethod deliveryMethod, decimal subTotal, string paymentIntentId)
        {
            PaymentIntentId = paymentIntentId;
            BuyerEmail = buyerEmail;  
            Items = items; 
            ShipToAddress = shipToAddress;
            DeliveryMethod = deliveryMethod;
            SubTotal = subTotal;
            DeliveryMethodId = deliveryMethod.Id;
        }

        public string PaymentIntentId { get; set; }

        public string BuyerEmail { get; set; }

        //OrderDate
        public DateTimeOffset DateTimeOffset { get; private set; } = DateTimeOffset.UtcNow;

        public ICollection<OrderItem> Items { get; private set; } = [];

        public OrderAddress ShipToAddress { get; private set; } = default!;

        public DeliveryMethod DeliveryMethod { get; private set; } = default!;

        public int DeliveryMethodId { get;private set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public decimal SubTotal { get; private set; }

        public decimal GetTotal() => SubTotal + (DeliveryMethod?.Cost ?? 0m);

    }
}
