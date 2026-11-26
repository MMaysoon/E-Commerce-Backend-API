using E_commerce.Application.Common;
using E_commerce.Application.DTOs.Baskets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.IServices
{
    public interface IPaymentService
    {
        Task<Result<BasketDto>> CreateOrUpdatePaymentIntentAsync(string basketId,string basketToken,int deliveryMethodId,CancellationToken cancellation = default);

        Task PaymentSucceeded (string PaymentIntentId);
        Task PaymentFailed (string PaymentIntentId);


    }
}
