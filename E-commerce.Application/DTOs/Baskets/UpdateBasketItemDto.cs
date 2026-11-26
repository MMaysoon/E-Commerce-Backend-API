using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.DTOs.Baskets
{
    public class UpdateBasketItemDto
    {
        // update quantity
        [Range(1, 99)]
        public int Quantity { get; set; }
    }
}
