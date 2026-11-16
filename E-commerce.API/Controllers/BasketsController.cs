using E_commerce.Application.DTOs.Baskets;
using E_commerce.Application.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BasketsController :ApiBaseController
    {
        private readonly IBasketService _basketService;

        public BasketsController(IBasketService basketService)
        {
            _basketService = basketService;
        }

        //Get -> baseUrl/api/baskets/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BasketDto),StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BasketDto>> GetBasketAsync(string id, CancellationToken ct = default)
        {
            var result = await _basketService.GetBasketAsync(id, ct);
            return ToActionResult(result);
        }


        // post  -> baseUrl/api/baskets
        [HttpPost]
        public async Task<ActionResult<BasketDto>> CreateOrUpdateBasketAsync([FromBody] BasketDto basket, CancellationToken cancellationToken = default)
        {
            var result = await _basketService.CreateOrUpdateBasketAsync(basket, cancellationToken);
            return ToActionResult(result);
        }


        // delete -> baseUrl/api/baskets/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<bool>> DeleteBasketAsync(string id, CancellationToken cancellationToken = default)
        {
            var result = await _basketService.DeleteBasketAsync(id, cancellationToken);
            return ToActionResult(result);
        }
    }
}
