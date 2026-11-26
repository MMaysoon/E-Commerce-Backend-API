using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.DTOs.Product
{
    public class UpdateProductDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = default!;

        [Required]
        [StringLength(100)]
        public string Description { get; set; } = default!;

        [Required]
        [StringLength(200)]
        public string PictureUrl { get; set; } = default!;

        [Range(typeof(decimal), "0.01", "999999999")]
        public decimal Price { get; set; }

        [Range(1, int.MaxValue)]
        public int TypeId { get; set; }

        [Range(1, int.MaxValue)]
        public int BrandId { get; set; }
    }
}
