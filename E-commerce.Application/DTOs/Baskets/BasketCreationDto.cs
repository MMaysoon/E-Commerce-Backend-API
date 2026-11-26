using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.DTOs.Baskets
{
    public class BasketCreationDto
    {
        public BasketDto Basket { get; set; } = default!;

        public string BasketToken { get; set; } = default!;
    }
}
