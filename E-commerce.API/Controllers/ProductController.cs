using E_commerce.Application.Common;
using E_commerce.Application.DTOs.Product;
using E_commerce.Application.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController(IProductService productService) : ApiBaseController
    {
        //1.Get All
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResult<ProductDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PaginatedResult<ProductDto>>> GetAllProducts([FromQuery] ProductQueryParams queryParams, CancellationToken ct)
        {
            var products = await productService.GetAllProductsAsync(queryParams, ct);
            return ToActionResult(products);
        }

        
        [HttpGet("Search")]
        [ProducesResponseType(typeof(IReadOnlyList<ProductDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetAllProductsusingSearch([FromQuery]ProductQueryParams queryParams ,CancellationToken ct)
        {
            var products = await productService.GetAllProductsUsingSearchAsync(queryParams, ct);
            return ToActionResult(products);
        }

        //2.Get By Id
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ProductDto),StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductDto>> GetProduct(int id ,CancellationToken ct)
        {
            var product = await productService.GetProductAsync(id,ct);
            return ToActionResult(product);
        }

        //3.Get Brands
        [HttpGet("Brands")]
        public async Task<ActionResult<IReadOnlyList<BrandDto>>> GetAllBrands(CancellationToken ct)
            =>  ToActionResult(await productService.GetAllBrandsAsync(ct));


        //4.Get types
        [HttpGet("Types")]
        public async Task<ActionResult<IReadOnlyList<TypeDto>>> GetAllTypes(CancellationToken ct)
            => ToActionResult(await productService.GetAllTypesAsync(ct));

    }
}
