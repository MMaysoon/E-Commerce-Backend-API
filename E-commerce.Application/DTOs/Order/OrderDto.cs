
using System.ComponentModel.DataAnnotations;

namespace E_commerce.Application.DTOs.Order
{
    public class OrderDto
    {
        [Required]
        public string BasketId { get; set; } = default!;

        [Required]
        public string BasketToken { get; set; } = default!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;

        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = default!;

        [Required]
        [Range(1, int.MaxValue)]
        public int DeliveryMethodId { get; set; }

        [Required]
        public AddressDto ShipToAddress { get; set; } = default!;
    }
}
