using E_commerce.Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Contracts
{
    public interface IPaymentGateWay
    {
        Task<PaymentIntentResult> CreatePaymentIntentAsync(decimal amount, string currency, CancellationToken cancellation = default);
        Task<PaymentIntentResult> UpdatePaymentIntentAsync(string paymentIntentId,decimal amount, CancellationToken cancellation = default);


    }
}
