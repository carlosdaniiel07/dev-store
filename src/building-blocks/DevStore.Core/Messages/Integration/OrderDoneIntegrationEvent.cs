using System;

namespace DevStore.Core.Messages.Integration
{
    public class OrderDoneIntegrationEvent : IntegrationEvent
    {
        public Guid CustomerId { get; private set; }
        public override string Topic => "OrderDone";

        public OrderDoneIntegrationEvent(Guid customerId)
        {
            CustomerId = customerId;
        }
    }
}