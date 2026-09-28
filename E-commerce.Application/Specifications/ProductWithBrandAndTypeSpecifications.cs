using E_commerce.Domain.Entities.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Specifications
{
    public class ProductWithBrandAndTypeSpecifications:BaseSpecifications<Product, int>
    {
        
        //Constructor to include ProductBrand and ProductType in the specification
        public ProductWithBrandAndTypeSpecifications()
        {
            AddInclude(p => p.ProductBrand);
            AddInclude(p => p.ProductType);
        }
    }
}
