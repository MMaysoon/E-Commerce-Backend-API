
using E_commerce.Application.Common;
using E_commerce.Application.DTOs.Product;

namespace E_commerce.Application.IServices
{
    public interface IProductService
    {
        Task<Result<IReadOnlyList<ProductDto>>> GetAllProductsUsingSearchAsync(
            ProductQueryParams queryParams,
            CancellationToken ct = default);

        Task<Result<PaginatedResult<ProductDto>>> GetAllProductsAsync(
            ProductQueryParams queryParams,
            CancellationToken ct = default);

        Task<Result<ProductDto>> GetProductAsync(
            int id,
            CancellationToken ct = default);

        Task<Result<ProductDto>> UpdateProductAsync(
            int id,
            UpdateProductDto updateDto,
            CancellationToken ct = default);

        Task<Result<IReadOnlyList<BrandDto>>> GetAllBrandsAsync(
            CancellationToken ct = default);

        Task<Result<IReadOnlyList<TypeDto>>> GetAllTypesAsync(
            CancellationToken ct = default);
    }
}
