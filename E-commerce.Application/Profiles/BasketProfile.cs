using AutoMapper;
using E_commerce.Application.DTOs.Baskets;
using E_commerce.Domain.Entities.Baskets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Profiles
{
    public class BasketProfile :Profile
    {
        public BasketProfile()
        {
            CreateMap<CustomerBasket, BasketDto>();

            CreateMap<BasketItem, BasketItemDto>()
                .ForMember(
                    dest => dest.ProductId,  
                    opt => opt.MapFrom(src => src.Id)
                );
        }
    }
}
