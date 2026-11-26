
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
using System.Security.Cryptography;
using System.Text;
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
        private readonly IEmailService _emailService;

        public PaymentService(
            IBasketRepository basketRepo,
            IBasketService basketService,
            IUnitOfWork unitOfWork,
            IPaymentGateWay paymentGateway,
            IOptions<PaymentGatewaySettings> stripe,
            IMapper mapper,
            IEmailService emailService)
        {
            _basketRepo = basketRepo;
            _basketService = basketService;
            _unitOfWork = unitOfWork;
            _paymentGateway = paymentGateway;
            _stripe = stripe.Value;
            _mapper = mapper;
            _emailService = emailService;
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

            // Prevent sending the confirmation email again
            if (order.ConfirmationEmailSent)
                return;

            // Update order status
            order.Status = OrderStatus.PaymentReceived;

            // Generate a separate tracking token for the email
            var emailTrackingToken = GenerateOrderTrackingToken();

            // Store only the token hash in the database
            order.EmailTrackingTokenHash =
                HashOrderTrackingToken(emailTrackingToken);

            await _unitOfWork.SaveChangesAsync();

            // Build confirmation email
            var emailBody = BuildOrderConfirmationEmail(
                order,
                emailTrackingToken);

            // Send confirmation email
            await _emailService.SendEmailAsync(
                order.BuyerEmail,
                "Order Confirmation - Payment Successful",
                emailBody);

            // Mark confirmation email as sent
            order.ConfirmationEmailSent = true;

            await _unitOfWork.SaveChangesAsync();
        }

        private string BuildOrderConfirmationEmail(Order order,string trackingToken)
        {
            var itemsHtml = string.Join("", order.Items.Select(item => $@"
                <tr>
                    <td style='padding:12px;border-bottom:1px solid #eee;'>
                        {System.Net.WebUtility.HtmlEncode(item.Product.ProductName)}
                    </td>
                    <td style='padding:12px;border-bottom:1px solid #eee;text-align:center;'>
                        {item.Quantity}
                    </td>
                    <td style='padding:12px;border-bottom:1px solid #eee;text-align:right;'>
                        {item.Price:C}
                    </td>
                    <td style='padding:12px;border-bottom:1px solid #eee;text-align:right;'>
                        {item.Price * item.Quantity:C}
                    </td>
                </tr>
            "));

            var deliveryCost = order.DeliveryMethod.Cost;
            var total = order.GetTotal();

            return $@"
                <!DOCTYPE html>
                <html>
                <body style='font-family:Arial,sans-serif;background:#f5f5f5;padding:20px;'>
                    <div style='max-width:650px;margin:auto;background:white;padding:30px;border-radius:10px;'>

                        <h2 style='color:#16a34a;'>Payment Successful!</h2>

                        <p>
                            Thank you for your order.
                            Your payment has been received successfully.
                        </p>

                        <h3>Order Information</h3>

                        <p><strong>Order ID:</strong> {order.Id}</p>
                        <p><strong>Order Date:</strong> {order.DateTimeOffset:dd MMM yyyy}</p>
                        <p><strong>Payment Status:</strong> Paid</p>
                        <p><strong>Order Status:</strong> {order.Status}</p>

                        <h3>Order Items</h3>

                        <table style='width:100%;border-collapse:collapse;'>
                            <thead>
                                <tr style='background:#f3f4f6;'>
                                    <th style='padding:12px;text-align:left;'>Product</th>
                                    <th style='padding:12px;'>Quantity</th>
                                    <th style='padding:12px;text-align:right;'>Price</th>
                                    <th style='padding:12px;text-align:right;'>Total</th>
                                </tr>
                            </thead>
                            <tbody>
                                {itemsHtml}
                            </tbody>
                        </table>

                        <h3>Payment Summary</h3>

                        <p>Subtotal: {order.SubTotal:C}</p>
                        <p>Delivery: {deliveryCost:C}</p>
                        <h3>Total: {total:C}</h3>

                        <h3>Secure Order Tracking</h3>

                        <p>
                            You can securely track your order using
                            the following information:
                        </p>

                        <p><strong>Order ID:</strong> {order.Id}</p>

                        <p><strong>Tracking Access Token:</strong></p>

                        <p style='word-break:break-all;background:#f3f4f6;padding:12px;'>
                            {System.Net.WebUtility.HtmlEncode(trackingToken)}
                        </p>

                        <p>
                            Keep this token private.
                            It is required to access your order details.
                        </p>

                        <p>Thank you for shopping with us!</p>

                    </div>
                </body>
                </html>";
        }

        private static string GenerateOrderTrackingToken()
        {
            var tokenBytes = RandomNumberGenerator.GetBytes(32);

            return Convert.ToBase64String(tokenBytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private static string HashOrderTrackingToken(string token)
        {
            var tokenBytes = Encoding.UTF8.GetBytes(token);

            var hashBytes = SHA256.HashData(tokenBytes);

            return Convert.ToHexString(hashBytes);
        }
    }
}
