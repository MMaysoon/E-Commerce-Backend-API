
using AutoMapper;
using E_commerce.Application.DTOs.Order;
using E_commerce.Domain.Entities.Order;

namespace E_commerce.Application.Profiles
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<OrderAddress, AddressDto>().ReverseMap();

            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(d => d.ProductId,
                    o => o.MapFrom(s => s.Product.Id))
                .ForMember(d => d.ProductName,
                    o => o.MapFrom(s => s.Product.ProductName))
                .ForMember(d => d.PictureUrl,
                    o => o.MapFrom<OrderItemPictureUrlResolver>());

            CreateMap<Order, OrderToReturnDto>()
                .ForMember(d => d.OrderDate,
                    o => o.MapFrom(s => s.DateTimeOffset))
                .ForMember(d => d.DeliveryMethod,
                    o => o.MapFrom(s => s.DeliveryMethod.ShortName))
                .ForMember(d => d.DeliveryCost,
                    o => o.MapFrom(s => s.DeliveryMethod.Cost))
                .ForMember(d => d.Status,
                    o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.Total,
                    o => o.MapFrom(s => s.GetTotal()));

            CreateMap<DeliveryMethod, DeliveryMethodDto>();
        }
    }
}
