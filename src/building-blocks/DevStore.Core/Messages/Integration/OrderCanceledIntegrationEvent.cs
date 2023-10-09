using System;

namespace DevStore.Core.Messages.Integration
{
    public class OrderCanceledIntegrationEvent : IntegrationEvent
    {
        public Guid CustomerId { get; private set; }
        public Guid OrderId { get; private set; }
        public override string Topic => "OrderCanceled";

        public OrderCanceledIntegrationEvent(Guid customerId, Guid orderId)
        {
            CustomerId = customerId;
            OrderId = orderId;
        }
    }
}