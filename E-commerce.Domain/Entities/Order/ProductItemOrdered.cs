using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Domain.Entities.Order
{
    public class ProductItemOrdered
    {
        public int Id { get; set; }  // = ProductId

        public string ProductName { get; set; } = default!;

        public string PictureUrl { get; set; } = default!;
    }
}
