using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Common
{
    public class ProductQueryParams
    {
        [Range(1, int.MaxValue)]
        public int? BrandId { get; set; }

        [Range(1, int.MaxValue)]
        public int? TypeId { get; set; }

        public string? SearchValue { get; set; }

        [EnumDataType(typeof(ProductSortingOptions))]
        public ProductSortingOptions Sort { get; set; }


        //Pagination
        [Range(1, int.MaxValue / MaxPageSize + 1)]
        public int PageIndex { get; set; } = 1;

        private const int DefaultPageSize = 5;
        private const int MaxPageSize = 10;

        private int pageSize = DefaultPageSize;
        public int PageSize
        {
            get => pageSize;
            set => pageSize = value > MaxPageSize ? MaxPageSize : (value < 1 ? DefaultPageSize : value);
        }


    }
}
