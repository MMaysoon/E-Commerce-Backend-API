
using E_commerce.API.Attributes;
using E_commerce.Application.Common;
using E_commerce.Application.DTOs.Product;
using E_commerce.Application.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController(
        IProductService productService) : ApiBaseController
    {
        [HttpGet]
        [RedisCache(100, "products")]
        [ProducesResponseType( typeof(PaginatedResult<ProductDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PaginatedResult<ProductDto>>> GetAllProducts( [FromQuery] ProductQueryParams queryParams,CancellationToken ct)
        {
            var products = await productService.GetAllProductsAsync(queryParams, ct);

            return ToActionResult(products);
        }

        [HttpGet("Search")]
        [RedisCache(100, "products")]
        [ProducesResponseType( typeof(IReadOnlyList<ProductDto>),  StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetAllProductsUsingSearch( [FromQuery] ProductQueryParams queryParams, CancellationToken ct)
        {
            var products = await productService.GetAllProductsUsingSearchAsync( queryParams, ct);

            return ToActionResult(products);
        }

        [HttpGet("{id:int}")]
        [RedisCache(100, "products")]
        [ProducesResponseType( typeof(ProductDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductDto>> GetProduct( int id,CancellationToken ct)
        {
            var product = await productService.GetProductAsync(id, ct);
            return ToActionResult(product);
        }

        [HttpGet("Brands")]
        [RedisCache(300, "brands")]
        public async Task<ActionResult<IReadOnlyList<BrandDto>>> GetAllBrands(CancellationToken ct)
        {
            return ToActionResult(  await productService.GetAllBrandsAsync(ct));
        }

        [HttpGet("Types")]
        [RedisCache(300, "types")]
        public async Task<ActionResult<IReadOnlyList<TypeDto>>> GetAllTypes( CancellationToken ct)
        {
            return ToActionResult( await productService.GetAllTypesAsync(ct));
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType( typeof(ProductDto),StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductDto>> UpdateProduct( int id,[FromBody] UpdateProductDto updateDto, CancellationToken ct)
        {
            var result = await productService.UpdateProductAsync( id, updateDto, ct);

            return ToActionResult(result);
        }
    }
}
