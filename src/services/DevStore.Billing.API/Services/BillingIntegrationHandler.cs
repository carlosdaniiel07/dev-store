using DevStore.Billing.API.Models;
using DevStore.Core.DomainObjects;
using DevStore.Core.Messages.Integration;
using DevStore.MessageBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DevStore.Billing.API.Services
{
    public class BillingIntegrationHandler : BackgroundService
    {
        private readonly IMessageBus _bus;
        private readonly IServiceProvider _serviceProvider;

        public BillingIntegrationHandler(
                            IServiceProvider serviceProvider,
                            IMessageBus bus)
        {
            _serviceProvider = serviceProvider;
            _bus = bus;
        }

        private async Task SetSubscribersAsync(CancellationToken stoppingToken)
        {
            await _bus.SubscribeAsync<OrderCanceledIntegrationEvent>("OrderCanceled", "DevStore.Billing.API", CancelTransaction, stoppingToken);
            await _bus.SubscribeAsync<OrderLoweredStockIntegrationEvent>("OrderLoweredStock", "DevStore.Billing.API", CapturePayment, stoppingToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await SetSubscribersAsync(stoppingToken);
        }

        private async Task CancelTransaction(OrderCanceledIntegrationEvent message)
        {
            using var scope = _serviceProvider.CreateScope();

            var pagamentoService = scope.ServiceProvider.GetRequiredService<IBillingService>();

            var Response = await pagamentoService.CancelTransaction(message.OrderId);

            if (!Response.ValidationResult.IsValid)
                throw new DomainException($"Failed to cancel order payment {message.OrderId}");
        }

        private async Task CapturePayment(OrderLoweredStockIntegrationEvent message)
        {
            using var scope = _serviceProvider.CreateScope();

            var pagamentoService = scope.ServiceProvider.GetRequiredService<IBillingService>();

            var Response = await pagamentoService.GetTransaction(message.OrderId);

            if (!Response.ValidationResult.IsValid)
                throw new DomainException($"Error trying to get order payment {message.OrderId}");

            await _bus.PublishAsync(new OrderPaidIntegrationEvent(message.CustomerId, message.OrderId));
        }
    }
}