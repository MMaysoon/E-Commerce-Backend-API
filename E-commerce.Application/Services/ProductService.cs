
using AutoMapper;
using E_commerce.Application.Common;
using E_commerce.Application.DTOs.Product;
using E_commerce.Application.IServices;
using E_commerce.Application.Specifications;
using E_commerce.Domain.Contracts;
using E_commerce.Domain.Entities.Products;

namespace E_commerce.Application.Services
{
    public class ProductService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICacheService cacheService) : IProductService
    {
        public async Task<Result<IReadOnlyList<BrandDto>>> GetAllBrandsAsync( CancellationToken ct = default)
        {
            var brands = await unitOfWork.GetRepository<ProductBrand, int>().GetAllAsync(ct);

            return Result<IReadOnlyList<BrandDto>>.Ok( mapper.Map<IReadOnlyList<BrandDto>>(brands));
        }

        public async Task<Result<PaginatedResult<ProductDto>>> GetAllProductsAsync(ProductQueryParams queryParams, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(queryParams);

            var spec = new ProductWithBrandAndTypeSpecifications(queryParams);

            var repository = unitOfWork.GetRepository<Product, int>();

            var products = await repository.GetAllAsync(spec, ct);

            var totalCount = await repository.CountAsync( new ProductCountSpecifications(queryParams),
                ct);

            var data = mapper.Map<IReadOnlyList<ProductDto>>(products);

            return new PaginatedResult<ProductDto>( queryParams.PageIndex,queryParams.PageSize, totalCount, data);
        }

        public async Task<Result<IReadOnlyList<ProductDto>>> GetAllProductsUsingSearchAsync(ProductQueryParams queryParams, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(queryParams);

            var spec = new ProductWithBrandAndTypeSpecifications(queryParams);

            var repo = unitOfWork.GetRepository<Product, int>();

            var products = await repo.GetAllAsync(spec, ct);

            var data = mapper.Map<IReadOnlyList<ProductDto>>(products);

            return Result<IReadOnlyList<ProductDto>>.Ok(data);
        }

        public async Task<Result<IReadOnlyList<TypeDto>>> GetAllTypesAsync(CancellationToken ct = default)
        {
            var types = await unitOfWork.GetRepository<ProductType, int>().GetAllAsync(ct);

            return Result<IReadOnlyList<TypeDto>>.Ok( mapper.Map<IReadOnlyList<TypeDto>>(types));
        }

        public async Task<Result<ProductDto>> GetProductAsync( int id, CancellationToken ct = default)
        {
            if (id < 1)
            {
                return Result<ProductDto>.Fail( Error.Validation("Product.InvalidId","Product id must be greater than zero."));
            }

            var spec = new ProductWithBrandAndTypeSpecifications(id);

            var product = await unitOfWork.GetRepository<Product, int>() .GetByIdAsync(spec, ct);

            if (product is null)
            {
                return Result<ProductDto>.Fail(
                    Error.NotFound( "Product.NotFound",  $"Product with id {id} was not found."));
            }

            var data = mapper.Map<ProductDto>(product);

            return data;
        }

        public async Task<Result<ProductDto>> UpdateProductAsync( int id, UpdateProductDto updateDto,CancellationToken ct = default)
        {
            if (id < 1)
            {
                return Result<ProductDto>.Fail(Error.Validation("Product.InvalidId", "Product id must be greater than zero."));
            }

            ArgumentNullException.ThrowIfNull(updateDto);

            var repository = unitOfWork.GetRepository<Product, int>();

            var product = await repository.GetByIdAsync(id, ct);

            if (product is null)
            {
                return Result<ProductDto>.Fail( Error.NotFound( "Product.NotFound", $"Product with id {id} was not found."));
            }

            product.Name = updateDto.Name;
            product.Description = updateDto.Description;
            product.PictureUrl = updateDto.PictureUrl;
            product.Price = updateDto.Price;
            product.TypeId = updateDto.TypeId;
            product.BrandId = updateDto.BrandId;

            repository.Update(product);

            await unitOfWork.SaveChangesAsync(ct);

            // Invalidate all cached product GET endpoints
            await cacheService.InvalidateGroupAsync("products", ct);

            var updatedProduct = await unitOfWork .GetRepository<Product, int>() .GetByIdAsync(  new ProductWithBrandAndTypeSpecifications(id), ct);

            var data = mapper.Map<ProductDto>(updatedProduct);

            return Result<ProductDto>.Ok(data);
        }
    }
}
