using E_commerce.Application.Common;
using E_commerce.Domain.Entities.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Specifications
{
    public class ProductWithBrandAndTypeSpecifications : BaseSpecifications<Product, int>
    {

        //Constructor to include ProductBrand and ProductType in the specification
        //Get All ( using Include)
        public ProductWithBrandAndTypeSpecifications() : base(null)
        {
            AddInclude(p => p.ProductBrand);
            AddInclude(p => p.ProductType);
        }

        //Get By Id (using criteria(the expression p => p.Id == id) and Include)
        public ProductWithBrandAndTypeSpecifications(int id) : base(p => p.Id == id)
        {
            AddInclude(p => p.ProductBrand);
            AddInclude(p => p.ProductType);
        }


        // Get All using Search (include & Criteria & Sorting)
        public ProductWithBrandAndTypeSpecifications(ProductQueryParams queryParams) : base
            (p =>
               // if chose these filtler must be applied to the query, otherwise ignore it
               (!queryParams.BrandId.HasValue || p.BrandId == queryParams.BrandId)
            &&
               (!queryParams.TypeId.HasValue || p.TypeId == queryParams.TypeId)
            &&
               (string.IsNullOrEmpty(queryParams.SearchValue) || p.Name.Contains(queryParams.SearchValue))
            )
        {
            AddInclude(p => p.ProductBrand);
            AddInclude(p => p.ProductType);


            // Sorting 
            switch (queryParams.Sort)
            {
                case ProductSortingOptions.NameAsc: AddOrderBy(p => p.Name); break;
                case ProductSortingOptions.NameDesc: AddOrderByDesc(p => p.Name); break;
                case ProductSortingOptions.PriceAsc: AddOrderBy(p => p.Price); break;
                case ProductSortingOptions.PriceDesc: AddOrderByDesc(p => p.Price); break;
                default: AddOrderBy(p => p.Id); break;
            }     
        }
    }

}
