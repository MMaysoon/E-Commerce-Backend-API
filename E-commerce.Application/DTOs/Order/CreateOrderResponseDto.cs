
namespace E_commerce.Application.DTOs.Order
{
    public class CreateOrderResponseDto
    {
        public OrderToReturnDto Order { get; set; } = default!;

        public string OrderAccessToken { get; set; } = default!;
    }
}
