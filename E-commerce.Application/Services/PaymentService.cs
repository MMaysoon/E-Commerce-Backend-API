
using AutoMapper;
using E_commerce.Application.Common;
using E_commerce.Application.Contracts;
using E_commerce.Application.DTOs.Baskets;
using E_commerce.Application.IServices;
using E_commerce.Application.Settings;
using E_commerce.Application.Specifications;
using E_commerce.Domain.Contracts;
using E_commerce.Domain.Entities.Baskets;
using E_commerce.Domain.Entities.Order;
using E_commerce.Domain.Entities.Products;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace E_commerce.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IBasketRepository _basketRepo;
        private readonly IBasketService _basketService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentGateWay _paymentGateway;
        private readonly PaymentGatewaySettings _stripe;
        private readonly IMapper _mapper;

        public PaymentService(
            IBasketRepository basketRepo,
            IBasketService basketService,
            IUnitOfWork unitOfWork,
            IPaymentGateWay paymentGateway,
            IOptions<PaymentGatewaySettings> stripe,
            IMapper mapper)
        {
            _basketRepo = basketRepo;
            _basketService = basketService;
            _unitOfWork = unitOfWork;
            _paymentGateway = paymentGateway;
            _stripe = stripe.Value;
            _mapper = mapper;
        }

        public async Task<Result<BasketDto>> CreateOrUpdatePaymentIntentAsync(string basketId,string basketToken,int deliveryMethodId,CancellationToken cancellation = default)
        {
            // 1. Validate basket ownership
            var basketResult = await _basketService.GetBasketForCheckoutAsync(
                basketId,
                basketToken,
                cancellation);

            if (!basketResult.IsSuccess)
                return Result<BasketDto>.Fail(basketResult.Errors);

            CustomerBasket basket = basketResult.data;

            // 2. Get delivery method
            var deliveryRepo =
                _unitOfWork.GetRepository<DeliveryMethod, int>();

            var deliveryMethod = await deliveryRepo.GetByIdAsync(
                deliveryMethodId,
                cancellation);

            if (deliveryMethod == null)
            {
                return Result<BasketDto>.Fail(
                    Error.NotFound(
                        "DeliveryMethod.NotFound",
                        $"Delivery method with Id {deliveryMethodId} was not found."));
            }

            // 3. Get products and refresh their prices
            var productRepo = _unitOfWork.GetRepository<Product, int>();

            var productIds = basket.Items
                .Select(item => item.Id)
                .ToHashSet();

            var products = (await productRepo.GetAllAsync(
                new ProductsWithIdsSpecifications(productIds),
                cancellation))
                .ToDictionary(product => product.Id);

            foreach (var item in basket.Items)
            {
                if (!products.TryGetValue(item.Id, out var product))
                {
                    return Result<BasketDto>.Fail(
                        Error.NotFound(
                            "Product.NotFound",
                            $"Product with Id {item.Id} was not found."));
                }

                item.Price = product.Price;
            }

            // 4. Calculate amount using server-side prices
            basket.DeliveryMethodId = deliveryMethod.Id;
            basket.ShippingPrice = deliveryMethod.Cost;

            var subtotal = basket.Items.Sum(
                item => item.Quantity * item.Price);

            var total = subtotal + deliveryMethod.Cost;

            var amount = (long)Math.Round(
                total * 100m,
                MidpointRounding.AwayFromZero);

            // 5. Create or update Stripe PaymentIntent
            if (!string.IsNullOrWhiteSpace(basket.PaymentIntentId))
            {
                await _paymentGateway.UpdatePaymentIntentAsync(
                    basket.PaymentIntentId,
                    amount,
                    cancellation);
            }
            else
            {
                var paymentResult =
                    await _paymentGateway.CreatePaymentIntentAsync(
                        amount,
                        _stripe.DefaultCurrency,
                        cancellation);

                basket.PaymentIntentId = paymentResult.PaymentIntentId;
                basket.ClientSecret = paymentResult.ClientSecret;
            }

            // 6. Save updated basket in Redis
            await _basketRepo.CreateOrUpdateAsync(
                basket,
                ct: cancellation);

            return Result<BasketDto>.Ok(
                _mapper.Map<BasketDto>(basket));
        }

        public async Task PaymentFailed(string paymentIntentId)
        {
            var orderRepo = _unitOfWork.GetRepository<Order, Guid>();

            var order = await orderRepo.GetByIdAsync(
                new PaymentIntentSpecifications(paymentIntentId));

            if (order == null)
                return;

            order.Status = OrderStatus.PaymentFailed;

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task PaymentSucceeded(string paymentIntentId)
        {
            var orderRepo = _unitOfWork.GetRepository<Order, Guid>();

            var order = await orderRepo.GetByIdAsync(
                new PaymentIntentSpecifications(paymentIntentId));

            if (order == null)
                return;

            order.Status = OrderStatus.PaymentReceived;

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
