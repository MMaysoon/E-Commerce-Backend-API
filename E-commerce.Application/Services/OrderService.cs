
using AutoMapper;
using E_commerce.Application.Common;
using E_commerce.Application.DTOs.Order;
using E_commerce.Application.IServices;
using E_commerce.Application.Specifications;
using E_commerce.Domain.Contracts;
using E_commerce.Domain.Entities.Baskets;
using E_commerce.Domain.Entities.Order;
using E_commerce.Domain.Entities.Products;
using System.Security.Cryptography;
using System.Text;

namespace E_commerce.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBasketService _basketService;
        private readonly IMapper _mapper;

        public OrderService(IUnitOfWork unitOfWork,IBasketService basketService,IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _basketService = basketService;
            _mapper = mapper;
        }

        public async Task<Result<CreateOrderResponseDto>> CreateOrderAsync(OrderDto orderDto,CancellationToken cancellation = default)
        {
            // 1. Get and validate the basket ownership
            var basketResult = await _basketService.GetBasketForCheckoutAsync(orderDto.BasketId,orderDto.BasketToken,cancellation);

            if (!basketResult.IsSuccess)
                return Result<CreateOrderResponseDto>.Fail(basketResult.Errors);

            CustomerBasket basket = basketResult.data;

            // 2. Check PaymentIntent
            if (string.IsNullOrWhiteSpace(basket.PaymentIntentId))
            {
                return Result<CreateOrderResponseDto>.Fail(
                    Error.Validation(
                        "PaymentIntent.Missing",
                        "Payment must be initialized before creating the order."));
            }

            // 3. Get repositories
            var orderRepo = _unitOfWork.GetRepository<Order, Guid>();
            var productRepo = _unitOfWork.GetRepository<Product, int>();

            // 4. Prevent duplicate orders for the same PaymentIntent
            var existingOrder = await orderRepo.GetByIdAsync(new PaymentIntentSpecifications(basket.PaymentIntentId),cancellation);

            if (existingOrder is not null)
            {
                return Result<CreateOrderResponseDto>.Fail(
                    Error.Failure(
                        "Order.AlreadyExists",
                        "An order already exists for this payment."));
            }

            // 5. Get products from database
            var productIds = basket.Items.Select(item => item.Id).ToHashSet();

            var products = (await productRepo.GetAllAsync(new ProductsWithIdsSpecifications(productIds),cancellation)).ToDictionary(product => product.Id);

            var orderItems = new List<OrderItem>(basket.Items.Count);

            foreach (var item in basket.Items)
            {
                if (!products.TryGetValue(item.Id, out var product))
                {
                    return Result<CreateOrderResponseDto>.Fail(
                        Error.NotFound(
                            "Product.NotFound",
                            $"Product with Id {item.Id} was not found."));
                }

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

            // 6. Get delivery method
            var deliveryRepo = _unitOfWork.GetRepository<DeliveryMethod, int>();

            var deliveryMethod = await deliveryRepo.GetByIdAsync(orderDto.DeliveryMethodId,cancellation);

            if (deliveryMethod == null)
            {
                return Result<CreateOrderResponseDto>.Fail(
                    Error.NotFound(
                        "DeliveryMethod.NotFound",
                        $"Delivery method with Id {orderDto.DeliveryMethodId} was not found."));
            }

            // 7. Calculate subtotal using database product prices
            var subTotal = orderItems.Sum(item => item.Quantity * item.Price);

            // 8. Create shipping address
            var orderAddress = _mapper.Map<OrderAddress>( orderDto.ShipToAddress);

            orderAddress.PhoneNumber = orderDto.PhoneNumber;

            // 9. Generate secure OrderAccessToken
            var orderAccessToken = GenerateOrderAccessToken();

            var orderAccessTokenHash = HashOrderAccessToken(
                orderAccessToken);

            // 10. Create pending order
            var order = new Order(
                orderDto.Email,
                orderDto.PhoneNumber,
                orderItems,
                orderAddress,
                deliveryMethod,
                subTotal,
                basket.PaymentIntentId,
                orderAccessTokenHash);

            orderRepo.Add(order);

            // 11. Save order in SQL Server
            var result = await _unitOfWork.SaveChangesAsync(cancellation);

            if (result <= 0)
            {
                return Result<CreateOrderResponseDto>.Fail(
                    Error.Failure(
                        "Order.SaveFailed",
                        "Cannot create order."));
            }

            // 12. Delete basket after successful order creation
            await _basketService.DeleteBasketAsync( orderDto.BasketId, orderDto.BasketToken,cancellation);

            // 13. Return order + raw token once
            var response = new CreateOrderResponseDto
            {
                Order = _mapper.Map<OrderToReturnDto>(order),
                OrderAccessToken = orderAccessToken
            };

            return Result<CreateOrderResponseDto>.Ok(response);
        }

        public async Task<Result<IReadOnlyList<DeliveryMethodDto>>>GetAllDeliveryMethodAsync(CancellationToken cancellation = default)
        {
            var deliveryMethods = await _unitOfWork.GetRepository<DeliveryMethod, int>().GetAllAsync(cancellation);

            return Result<IReadOnlyList<DeliveryMethodDto>>.Ok(_mapper.Map<IReadOnlyList<DeliveryMethodDto>>( deliveryMethods));
        }

        private static string GenerateOrderAccessToken()
        {
            var tokenBytes = RandomNumberGenerator.GetBytes(32);

            return Convert.ToBase64String(tokenBytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private static string HashOrderAccessToken(string token)
        {
            var tokenBytes = Encoding.UTF8.GetBytes(token);

            var hashBytes = SHA256.HashData(tokenBytes);

            return Convert.ToHexString(hashBytes);
        }
    }
}
