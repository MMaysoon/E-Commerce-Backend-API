
using E_commerce.Application.DTOs.Baskets;
using E_commerce.Application.IServices;
using E_commerce.Application.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;

namespace E_commerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ApiBaseController
    {
        private readonly IPaymentService _paymentService;
        private readonly PaymentGatewaySettings _stripeSettings;

        public PaymentController( IPaymentService paymentService,IOptions<PaymentGatewaySettings> options)
        {
            _paymentService = paymentService;
            _stripeSettings = options.Value;
        }

        [AllowAnonymous]
        [HttpPost("{basketId}")]
        [ProducesResponseType(typeof(BasketDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<BasketDto>> CreateOrUpdatePaymentIntent(
            string basketId,
            [FromHeader(Name = "X-Basket-Token")] string basketToken,
            [FromQuery] int deliveryMethodId,
            CancellationToken cancellationToken)
        {
            return ToActionResult(
                await _paymentService.CreateOrUpdatePaymentIntentAsync(
                    basketId,
                    basketToken,
                    deliveryMethodId,
                    cancellationToken));
        }

        //[AllowAnonymous]
        //[HttpPost("webhook")]
        //public async Task<IActionResult> StripeWebhook()
        //{
        //    var json = await new StreamReader(
        //        HttpContext.Request.Body).ReadToEndAsync();

        //    try
        //    {
        //        var stripeEvent = EventUtility.ConstructEvent(
        //            json,
        //            Request.Headers["Stripe-Signature"],
        //            _stripeSettings.WebhookSecret);

        //        switch (stripeEvent.Type)
        //        {
        //            case EventTypes.PaymentIntentSucceeded:

        //                var succeededPaymentIntent =
        //                    stripeEvent.Data.Object as PaymentIntent;

        //                if (succeededPaymentIntent is not null)
        //                {
        //                    await _paymentService.PaymentSucceeded(
        //                        succeededPaymentIntent.Id);
        //                }

        //                break;

        //            case EventTypes.PaymentIntentPaymentFailed:

        //                var failedPaymentIntent =
        //                    stripeEvent.Data.Object as PaymentIntent;

        //                if (failedPaymentIntent is not null)
        //                {
        //                    await _paymentService.PaymentFailed(
        //                        failedPaymentIntent.Id);
        //                }

        //                break;
        //        }

        //        return Ok();
        //    }
        //    catch (StripeException ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}


        [AllowAnonymous]
        [HttpPost("webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(
                HttpContext.Request.Body).ReadToEndAsync();

            Console.WriteLine("Webhook received!");

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    _stripeSettings.WebhookSecret,
                    throwOnApiVersionMismatch: false);

                Console.WriteLine($"Event Type: {stripeEvent.Type}");

                switch (stripeEvent.Type)
                {
                    case EventTypes.PaymentIntentSucceeded:

                        var succeededPaymentIntent =
                            stripeEvent.Data.Object as PaymentIntent;

                        if (succeededPaymentIntent is not null)
                        {
                            Console.WriteLine(
                                $"Payment Succeeded: {succeededPaymentIntent.Id}");

                            await _paymentService.PaymentSucceeded(
                                succeededPaymentIntent.Id);
                        }

                        break;

                    case EventTypes.PaymentIntentPaymentFailed:

                        var failedPaymentIntent =
                            stripeEvent.Data.Object as PaymentIntent;

                        if (failedPaymentIntent is not null)
                        {
                            await _paymentService.PaymentFailed(
                                failedPaymentIntent.Id);
                        }

                        break;
                }

                Console.WriteLine("Webhook processed successfully!");

                return Ok();
            }
            catch (StripeException ex)
            {
                Console.WriteLine($"Stripe Exception: {ex}");

                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Exception: {ex}");

                return StatusCode(500);
            }
        }
    }
}
