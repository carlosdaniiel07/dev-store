using System;

namespace DevStore.Core.Messages.Integration
{
    public class OrderPaidIntegrationEvent : IntegrationEvent
    {
        public Guid CustomerId { get; private set; }
        public Guid OrderId { get; private set; }
        public override string Topic => "OrderPaid";

        public OrderPaidIntegrationEvent(Guid customerId, Guid orderId)
        {
            CustomerId = customerId;
            OrderId = orderId;
        }
    }
}