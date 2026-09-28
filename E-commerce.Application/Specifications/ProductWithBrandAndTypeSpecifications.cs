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
        //Get All ( using Include)
        public ProductWithBrandAndTypeSpecifications():base(null)
        {
            AddInclude(p => p.ProductBrand);
            AddInclude(p => p.ProductType);
        }

        //Get By Id (using criteria(the expression p => p.Id == id) and Include)
        public ProductWithBrandAndTypeSpecifications(int id):base(p => p.Id == id)
        {
            AddInclude(p => p.ProductBrand);
            AddInclude(p => p.ProductType);
        }
    }


}
