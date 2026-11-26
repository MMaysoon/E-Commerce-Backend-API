using E_commerce.Domain.Entities.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Specifications
{
    public class ProductsWithIdsSpecifications : BaseSpecifications<Product, int>
    {
        public ProductsWithIdsSpecifications(HashSet<int> productIds) : base(p => productIds.Contains(p.Id))
        {

        }
    }
}
