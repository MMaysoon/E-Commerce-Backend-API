
using E_commerce.Domain.Common;

namespace E_commerce.Domain.Entities.Order
{
    public class Order : BaseEntity<Guid>
    {
        public Order()
        {
        }

        public Order(
            string buyerEmail,
            string phoneNumber,
            ICollection<OrderItem> items,
            OrderAddress shipToAddress,
            DeliveryMethod deliveryMethod,
            decimal subTotal,
            string paymentIntentId,
            string orderAccessTokenHash)
        {
            BuyerEmail = buyerEmail;
            PhoneNumber = phoneNumber;
            Items = items;
            ShipToAddress = shipToAddress;
            DeliveryMethod = deliveryMethod;
            SubTotal = subTotal;
            DeliveryMethodId = deliveryMethod.Id;
            PaymentIntentId = paymentIntentId;
            OrderAccessTokenHash = orderAccessTokenHash;
        }

        public string PaymentIntentId { get; set; } = default!;

        public string BuyerEmail { get; set; } = default!;

        public string PhoneNumber { get; set; } = default!;

        // Used for secure guest order tracking
        public string OrderAccessTokenHash { get; set; } = default!;

        public DateTimeOffset DateTimeOffset { get; private set; }
            = DateTimeOffset.UtcNow;

        public ICollection<OrderItem> Items { get; private set; } = [];

        public OrderAddress ShipToAddress { get; private set; } = default!;

        public DeliveryMethod DeliveryMethod { get; private set; } = default!;

        public int DeliveryMethodId { get; private set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public decimal SubTotal { get; private set; }

        public decimal GetTotal()
            => SubTotal + (DeliveryMethod?.Cost ?? 0m);
    }
}
