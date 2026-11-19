using E_commerce.Application.Common;
using E_commerce.Application.DTOs.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.IServices
{
    public interface IOrderService
    {
        Task<Result<OrderToReturnDto>> createOrderAsync(OrderDto orderDto, string email, CancellationToken cancellation = default);

        Task<Result<IReadOnlyList<DeliveryMethodDto>>> GetAllDeliveryMethodAsync (CancellationToken cancellation = default);

        Task<Result<IReadOnlyList<OrderToReturnDto>>> GetAllOrderAsync (string email, CancellationToken cancellation = default);

        Task<Result<OrderToReturnDto>> GetOrderByIdEmailAsync(Guid id , string email, CancellationToken cancellation = default);
    }
}
