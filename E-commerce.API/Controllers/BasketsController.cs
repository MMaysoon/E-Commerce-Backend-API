
using E_commerce.Application.DTOs.Baskets;
using E_commerce.Application.IServices;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BasketsController : ApiBaseController
    {
        private readonly IBasketService _basketService;

        public BasketsController(IBasketService basketService)
        {
            _basketService = basketService;
        }

        [HttpPost]
        public async Task<ActionResult<BasketCreationDto>> CreateBasket( CancellationToken ct)
        {
            var result = await _basketService.CreateBasketAsync(ct);
            return ToActionResult(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BasketDto>> GetBasket(string id,[FromHeader(Name = "X-Basket-Token")] string basketToken,CancellationToken ct)
        {
            var result = await _basketService.GetBasketAsync(id, basketToken, ct);

            return ToActionResult(result);
        }

        [HttpPost("{id}/items")]
        public async Task<ActionResult<BasketDto>> AddItem(string id,[FromHeader(Name = "X-Basket-Token")] string basketToken,[FromBody] AddBasketItemDto itemDto,CancellationToken ct)
        {
            var result = await _basketService.AddItemAsync(
                id, basketToken, itemDto, ct);

            return ToActionResult(result);
        }

        [HttpPut("{id}/items/{productId}")]
        public async Task<ActionResult<BasketDto>> UpdateItem(string id,int productId,[FromHeader(Name = "X-Basket-Token")] string basketToken,[FromBody] UpdateBasketItemDto itemDto,CancellationToken ct)
        {
            var result = await _basketService.UpdateItemAsync(
                id, basketToken, productId, itemDto, ct);

            return ToActionResult(result);
        }

        [HttpDelete("{id}/items/{productId}")]
        public async Task<ActionResult<BasketDto>> RemoveItem(string id,int productId, [FromHeader(Name = "X-Basket-Token")] string basketToken, CancellationToken ct)
        {
            var result = await _basketService.RemoveItemAsync( id, basketToken, productId, ct);

            return ToActionResult(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<bool>> DeleteBasket(string id,[FromHeader(Name = "X-Basket-Token")] string basketToken,CancellationToken ct)
        {
            var result = await _basketService.DeleteBasketAsync( id, basketToken, ct);

            return ToActionResult(result);
        }
    }
}
