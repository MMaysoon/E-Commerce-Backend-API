using AutoMapper;
using E_commerce.Application.DTOs.Order;
using E_commerce.Domain.Entities.Order;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Profiles
{
    public class OrderItemPictureUrlResolver(IOptions<UrlSettings>options)
        :IValueResolver<OrderItem,OrderItemDto,string>
    {
        private readonly UrlSettings _urlSettings = options.Value;

        public string Resolve (OrderItem source , OrderItemDto destination ,string destMember,ResolutionContext Context)
        {
            if (string.IsNullOrEmpty(source.Product.PictureUrl))
                return string.Empty;

            return $"{_urlSettings.BaseUrl}{source.Product.PictureUrl}";
        }
    }
}
