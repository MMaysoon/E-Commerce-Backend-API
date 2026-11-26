
using E_commerce.Application.DTOs.Order;
using E_commerce.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ApiBaseController
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(typeof(CreateOrderResponseDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<CreateOrderResponseDto>> CreateOrder(OrderDto orderDto,CancellationToken cancellationToken)
        {
            return ToActionResult(await _orderService.CreateOrderAsync(orderDto,cancellationToken));
        }

        [AllowAnonymous]
        [HttpGet("deliveryMethods")]
        public async Task<ActionResult<IReadOnlyList<DeliveryMethodDto>>> GetDeliveryMethods(CancellationToken cancellationToken)
        {
            return ToActionResult(await _orderService.GetAllDeliveryMethodAsync(cancellationToken));
        }

        [AllowAnonymous]
        [HttpGet("{orderId:guid}/track")]
        public async Task<ActionResult<OrderToReturnDto>> TrackOrder(Guid orderId,[FromHeader(Name = "X-Order-Access-Token")] string accessToken,CancellationToken cancellationToken)
        {
            return ToActionResult(
                await _orderService.TrackOrderAsync(
                    orderId,
                    accessToken,
                    cancellationToken));
        }
    }
}
