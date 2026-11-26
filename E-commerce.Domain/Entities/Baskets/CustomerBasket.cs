using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Domain.Entities.Baskets
{
    public class CustomerBasket
    {
        public string Id { get; set; } = default!;
        public ICollection<BasketItem> Items { get; set; } = [];

        // Used to verfiy basket ownership
        public string? BasketTokenHash { get; set; }


        // Stripe Data
        public string? ClientSecret { get; set; }
        public string? PaymentIntentId { get; set; }

        // Delivery Data
        public int? DeliveryMethodId { get; set; }
        public decimal? ShippingPrice { get; set; }
    }
}
