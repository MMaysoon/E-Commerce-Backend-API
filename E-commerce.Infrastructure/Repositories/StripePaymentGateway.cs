using E_commerce.Application.Common;
using E_commerce.Application.Contracts;
using E_commerce.Application.Settings;
using Microsoft.Extensions.Options;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace E_commerce.Infrastructure.Repositories
{
   public class StripePaymentGateway :IPaymentGateWay
   {
        // built in stripe method
        private readonly PaymentIntentService _paymentIntentService = new();

        public StripePaymentGateway(IOptions<PaymentGatewaySettings>options)
        {
            StripeConfiguration.ApiKey = options.Value.SecretKey;
        }

        public async  Task<PaymentIntentResult> CreatePaymentIntentAsync(decimal amount, string currency, CancellationToken cancellation = default)
        {
            var options = new PaymentIntentCreateOptions
             {
                Amount = (long)amount,
                Currency = currency.ToLowerInvariant(),
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                    AllowRedirects = "never"
                }
             };

            var intent = await _paymentIntentService.CreateAsync(options,cancellationToken :cancellation);
            return new PaymentIntentResult(intent.Id, intent.ClientSecret);
        }

        public async Task<PaymentIntentResult> UpdatePaymentIntentAsync(string paymentIntentId, decimal amount, CancellationToken cancellation = default)
        {
            var options = new PaymentIntentUpdateOptions { Amount = (long)amount };
            var intent = await _paymentIntentService.UpdateAsync(paymentIntentId, options, cancellationToken:cancellation);
            return new PaymentIntentResult(intent.Id, intent.ClientSecret);
        }
    }
}
