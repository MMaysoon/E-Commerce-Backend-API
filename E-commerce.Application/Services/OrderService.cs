using AutoMapper;
using E_commerce.Application.Common;
using E_commerce.Application.DTOs.Order;
using E_commerce.Application.IServices;
using E_commerce.Application.Specifications;
using E_commerce.Domain.Contracts;
using E_commerce.Domain.Entities.Order;
using E_commerce.Domain.Entities.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace E_commerce.Application.Services
{
    public class OrderService(IUnitOfWork unitOfWork, IBasketRepository basketRepo, IMapper mapper) : IOrderService
    {
        public async Task<Result<OrderToReturnDto>> createOrderAsync(OrderDto orderDto, string email, CancellationToken cancellation = default)
        {
            var basket = await basketRepo.GetBasketAsync(orderDto.BasketId, cancellation);

            if (basket == null)
                return Result<OrderToReturnDto>.Fail(Error.NotFound("Basket not found", $"Basket with Id {orderDto.BasketId} is not found"));

            if (basket.Items.Count == 0)
                return Result<OrderToReturnDto>.Fail(Error.Validation("Basket is empty", $"can no create order with basket with id {orderDto.BasketId}"));

            var orderRepo =  unitOfWork.GetRepository<Order, Guid>();
            var productRepo = unitOfWork.GetRepository<Product, int>();

            var exsitingOrder = await orderRepo.GetByIdAsync(new PaymentIntentSpecifications(basket.PaymentIntentId), cancellation);

            if (exsitingOrder is not null)
                orderRepo.Delete(exsitingOrder);

            var productIds= basket.Items.Select(i=>i.Id).ToHashSet();

            var products = (await productRepo.GetAllAsync(new ProductsWithIdsSpecifications(productIds), cancellation)).ToDictionary(x => x.Id);

            var orderItems = new List<OrderItem>(basket.Items.Count);

            foreach (var item in basket.Items)
            {
                if (!products.TryGetValue(item.Id, out var product))
                    return Result<OrderToReturnDto>.Fail(Error.NotFound("Product Not Found", $"Product With Id {item.Id} Is Not Found "));

                orderItems.Add(new OrderItem
                {
                    Price = product.Price,
                    Quantity = item.Quantity,
                    Product = new ProductItemOrdered
                    {
                       Id = product.Id,
                        ProductName = product.Name,
                        PictureUrl = product.PictureUrl
                    }
                });
            }

            var orderAddress = mapper.Map<OrderAddress>(orderDto.ShipToAddress);
            var deliveryRepo = unitOfWork.GetRepository<DeliveryMethod, int>();
            var deliveryMethod = await deliveryRepo.GetByIdAsync(orderDto.DeliveryMethodId, cancellation);
            if (deliveryMethod == null)
                return Result<OrderToReturnDto>.Fail(Error.NotFound("Delivery Method Not Found", $"DeliveryMethod With Id {orderDto.DeliveryMethodId} Is Not Found "));

            var subTotal = orderItems.Sum(x => x.Quantity * x.Price);
            var order = new Order(email, orderItems, orderAddress, deliveryMethod, subTotal, basket.PaymentIntentId);

            orderRepo.Add(order);
            var result = await unitOfWork.SaveChangesAsync(cancellation);

            if (result <= 0)
            {
                return Result<OrderToReturnDto>.Fail(Error.Failure("Order Save Failed", "Cannot create order."));
            }

            await basketRepo.DeleteBasketAsync(orderDto.BasketId, cancellation);

            return mapper.Map<OrderToReturnDto>(order);
        }

        public async Task<Result<IReadOnlyList<DeliveryMethodDto>>> GetAllDeliveryMethodAsync(CancellationToken cancellation = default)
        {
            var deliveryMethods = await unitOfWork.GetRepository<DeliveryMethod, int>().GetAllAsync(cancellation);

            return Result<IReadOnlyList<DeliveryMethodDto>>.Ok(mapper.Map<IReadOnlyList<DeliveryMethodDto>>(deliveryMethods));
        }

        public async Task<Result<IReadOnlyList<OrderToReturnDto>>> GetAllOrderAsync(string email, CancellationToken cancellation = default)
        {
            var orders = await unitOfWork.GetRepository<Order, Guid>()
              .GetAllAsync(new OrderSpecfications(email), cancellation);

            return Result<IReadOnlyList<OrderToReturnDto>>.Ok(mapper.Map<IReadOnlyList<OrderToReturnDto>>(orders));
        }

        public async Task<Result<OrderToReturnDto>> GetOrderByIdEmailAsync(Guid id, string email, CancellationToken cancellation = default)
        {
            var order = await unitOfWork.GetRepository<Order, Guid>().GetByIdAsync(new OrderSpecfications(id, email), cancellation);
            if (order == null)
                return Result<OrderToReturnDto>.Fail(Error.NotFound("Order Not Found", $"Order With Id {id} Is Not Found"));

            return mapper.Map<OrderToReturnDto>(order);
        }
    }
}
