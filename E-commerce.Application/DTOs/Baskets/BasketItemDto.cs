using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.DTOs.Baskets
{
    public class BasketItemDto
    {
        [Required(ErrorMessage = "ProductId is required.")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "ProductName is required.")]
        public string ProductName { get; set; } = default!;

        public string PictureUrl { get; set; } = default!;

        [Range(1,double.MaxValue)]
        public decimal Price { get; set; }

        [Range(1,99)]
        public int Quantity { get; set; }
    }
}
