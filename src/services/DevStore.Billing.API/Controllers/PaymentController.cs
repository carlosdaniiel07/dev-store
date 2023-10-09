using DevStore.Billing.API.Models;
using DevStore.Billing.API.Services;
using DevStore.Core.Messages.Integration;
using DevStore.WebAPI.Core.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DevStore.Billing.API.Controllers
{
    public class PaymentController : MainController
    {
        [HttpPost("/payments/authorize")]
        public async Task<IActionResult> Authorize(OrderInitiatedIntegrationEvent orderInitiatedIntegrationEvent, [FromServices] IBillingService billingService)
        {
            var transaction = new Payment
            {
                OrderId = orderInitiatedIntegrationEvent.OrderId,
                PaymentType = (PaymentType)orderInitiatedIntegrationEvent.PaymentType,
                Amount = orderInitiatedIntegrationEvent.Amount,
                CreditCard = new CreditCard(
                    orderInitiatedIntegrationEvent.Holder,
                    orderInitiatedIntegrationEvent.CardNumber,
                    orderInitiatedIntegrationEvent.ExpirationDate,
                    orderInitiatedIntegrationEvent.SecurityCode
                ),
            };
            var result = await billingService.AuthorizeTransaction(transaction);

            return Ok(result);
        }
    }
}