using E_commerce.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Domain.Entities.Products
{
    public class Product : BaseEntity<int>
    {
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string PictureUrl { get; set; } = default!;

        public decimal Price { get; set; }


        //FK
        public int ProductTypeId { get; set; }
        public int ProductBrandId { get; set; }

        //Navigation properties
        public ProductType ProductType { get; set; } = default!;
        public ProductBrand ProductBrand { get; set; } = default!;


    }
}
