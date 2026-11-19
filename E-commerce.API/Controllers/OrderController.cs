using E_commerce.Application.DTOs.Order;
using E_commerce.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController :ApiBaseController
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [Authorize]
        [HttpPost]
        [ProducesResponseType(typeof(OrderToReturnDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<OrderToReturnDto>> CreateOrder(OrderDto orderDto, CancellationToken cancellationToken)
        => ToActionResult(await _orderService.createOrderAsync(orderDto, GetEmailFromToken(), cancellationToken));

        [AllowAnonymous]
        [HttpGet("deliveryMethods")]
        public async Task<ActionResult<IReadOnlyList<DeliveryMethodDto>>> GetDeliveryMethods(CancellationToken cancellationToken)
            => ToActionResult(await _orderService.GetAllDeliveryMethodAsync(cancellationToken));

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<OrderToReturnDto>>> GetAllOrders(CancellationToken cancellationToken)
            => ToActionResult(await _orderService.GetAllOrderAsync(GetEmailFromToken(), cancellationToken));

        [Authorize]
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(OrderToReturnDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderToReturnDto>> GetOrderById(Guid id, CancellationToken cancellationToken)
            => ToActionResult(await _orderService.GetOrderByIdEmailAsync(id, GetEmailFromToken(), cancellationToken));
    }
}
