namespace DevStore.Core.Messages.Integration
{
    public abstract class IntegrationEvent : Event
    {
        public abstract string Topic { get; }
    }
}