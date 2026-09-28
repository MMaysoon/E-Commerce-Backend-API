using AutoMapper;
using E_commerce.Application.Common;
using E_commerce.Application.DTOs.Product;
using E_commerce.Application.IServices;
using E_commerce.Application.Specifications;
using E_commerce.Domain.Contracts;
using E_commerce.Domain.Entities.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Services
{
    public class ProductService(IUnitOfWork unitOfWork, IMapper mapper) : IProductService
    {
        public async Task<Result<IReadOnlyList<BrandDto>>> GetAllBrandsAsync(CancellationToken ct = default)
        {

            var brands = await unitOfWork.GetRepository<ProductBrand, int>().GetAllAsync(ct);

            return Result<IReadOnlyList<BrandDto>>.Ok(mapper.Map<IReadOnlyList<BrandDto>>(brands));

        }

        public async Task<Result<IReadOnlyList<ProductDto>>> GetAllProductsAsync(CancellationToken ct = default)
        {
            //---1'st way
            //var products = await unitOfWork.GetRepository<Product,int>().GetAllAsync(ct);

            //return Result<IReadOnlyList<ProductDto>>.Ok(mapper.Map<IReadOnlyList<ProductDto>>(products));

            //---2'nd way
            //var repo =  unitOfWork.GetRepository<Product, int>();
            //var products = await repo.GetAllAsync(ct);
            //var data = mapper.Map<IReadOnlyList<ProductDto>>(products);
            //return Result<IReadOnlyList<ProductDto>>.Ok(data);

            //--------------------------
            // Get All with Specification DP
            var spec = new ProductWithBrandAndTypeSpecifications();
            var repo = unitOfWork.GetRepository<Product, int>();
            var products = await repo.GetAllAsync(spec,ct);
            var data = mapper.Map<IReadOnlyList<ProductDto>>(products);
            return Result<IReadOnlyList<ProductDto>>.Ok(data);


        }

        public async Task<Result<IReadOnlyList<ProductDto>>> GetAllProductsUsingSearchAsync(ProductQueryParams queryParams,CancellationToken ct = default)
        {
           
            var spec = new ProductWithBrandAndTypeSpecifications(queryParams);
            var repo = unitOfWork.GetRepository<Product, int>();
            var products = await repo.GetAllAsync(spec, ct);
            var data = mapper.Map<IReadOnlyList<ProductDto>>(products);
            return Result<IReadOnlyList<ProductDto>>.Ok(data);


        }


        public async Task<Result<IReadOnlyList<TypeDto>>> GetAllTypesAsync(CancellationToken ct = default)
        {
            var types = await unitOfWork.GetRepository<ProductType, int>().GetAllAsync(ct);

            return Result<IReadOnlyList<TypeDto>>.Ok(mapper.Map<IReadOnlyList<TypeDto>>(types));
        }

        public async Task<Result<ProductDto>> GetProductAsync(int id, CancellationToken ct = default)
        {
            var spec = new ProductWithBrandAndTypeSpecifications(id);

            var product = await unitOfWork.GetRepository<Product, int>().GetByIdAsync(spec,ct);

            if (product is null)
                return Result<ProductDto>.Fail(Error.NotFound("Product.NotFound", $"Product with id {id} was not found."));
              

            //Implicit conversion: TValue → Result<TValue>
            //return Result<ProductDto>.Ok(mapper.Map<ProductDto>(product));
            // ==
            var data= mapper.Map<ProductDto>(product);
            return data;
        }
    }
}
