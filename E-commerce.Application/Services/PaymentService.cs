using AutoMapper;
using E_commerce.Application.Common;
using E_commerce.Application.Contracts;
using E_commerce.Application.DTOs.Baskets;
using E_commerce.Application.IServices;
using E_commerce.Application.Settings;
using E_commerce.Application.Specifications;
using E_commerce.Domain.Contracts;
using E_commerce.Domain.Entities.Order;
using E_commerce.Domain.Entities.Products;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace E_commerce.Application.Services
{
    public class PaymentService :IPaymentService
    {
        private readonly IBasketRepository _basketRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentGateWay _paymentGateway;
        private readonly PaymentGatewaySettings _stripe;
        private readonly IMapper _mapper;

        public PaymentService(IBasketRepository basketRepo, IUnitOfWork unitOfWork, IPaymentGateWay paymentGateway, IOptions<PaymentGatewaySettings> stripe, IMapper mapper)
        {
            _basketRepo = basketRepo;
            _unitOfWork = unitOfWork;
            _paymentGateway = paymentGateway;
            _stripe = stripe.Value;
            _mapper = mapper;
        }

        public async Task<Result<BasketDto>> CreateOrUpdatePaymentIntentAsync(string basketId, CancellationToken cancellation = default)
        {
           var basket = await _basketRepo.GetBasketAsync(basketId,cancellation);


            if (basket == null)
                return Result<BasketDto>.Fail(Error.NotFound("Basket not found", $"Basket With Id {basketId} Is Not Found"));

            if (basket.Items.Count == 0)
                return Result<BasketDto>.Fail(Error.Validation("Basket is Empty", $"Can Not Create Order With Basket With Id {basketId}"));

            var productRepo = _unitOfWork.GetRepository<Product, int>();

            var productIds = basket.Items.Select(i => i.Id).ToHashSet();
            var products = (await productRepo.GetAllAsync(new ProductsWithIdsSpecifications(productIds), cancellation)).ToDictionary(x => x.Id);

            foreach (var item in basket.Items)
            {
                if (!products.TryGetValue(item.Id, out var product))
                    return Result<BasketDto>.Fail(Error.NotFound("Product Not Found", $"Product With Id {item.Id} Is Not Found "));

                item.Price = product.Price;
            }

            var deliveryRepo = _unitOfWork.GetRepository<DeliveryMethod, int>();
            var deliveryMethod = await deliveryRepo.GetByIdAsync(basket.DeliveryMethodId.Value, cancellation);
            if (deliveryMethod == null)
                return Result<BasketDto>.Fail(Error.NotFound("Delivery Method Not Found", $"DeliveryMethod With Id {basket.DeliveryMethodId} Is Not Found "));

            basket.ShippingPrice = deliveryMethod.Cost;
            var subtotal = basket.Items.Sum(i => i.Quantity * i.Price);
            var amount = (long)Math.Round((subtotal + deliveryMethod.Cost) * 100m);
            if (!string.IsNullOrEmpty(basket.PaymentIntentId))
            {
                await _paymentGateway.UpdatePaymentIntentAsync(basket.PaymentIntentId, amount, cancellation);
            }
            else
            {
                var result = await _paymentGateway.CreatePaymentIntentAsync(amount, _stripe.DefaultCurrency, cancellation);
                basket.PaymentIntentId = result.PaymentIntentId;
                basket.ClientSecret = result.ClientSecret;
            }
            await _basketRepo.CreateOrUpdateAsync(basket, ct: cancellation);
            return _mapper.Map<BasketDto>(basket);
           
        }

        public async Task PaymentFailed(string PaymentIntentId)
        {
            var orderRepo = _unitOfWork.GetRepository<Order, Guid>();

            var order = await orderRepo.GetByIdAsync(new PaymentIntentSpecifications (PaymentIntentId));

            if (order == null)
                return;

            order.Status= OrderStatus.PaymentFailed;

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task PaymentSuccessed(string PaymentIntentId)
        {
            var orderRepo = _unitOfWork.GetRepository<Order, Guid>();

            var order = await orderRepo.GetByIdAsync(new PaymentIntentSpecifications(PaymentIntentId));

            if (order == null)
                return;
            order.Status = OrderStatus.PaymentReceived;

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
