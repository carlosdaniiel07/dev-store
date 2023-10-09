using DevStore.Core.Messages.Integration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DevStore.MessageBus
{
    public interface IMessageBus
    {
        Task PublishAsync<T>(T message) where T : IntegrationEvent;
        Task SubscribeAsync<T>(string topic, string groupId, Func<T, Task> onMessage, CancellationToken cancellationToken) where T : IntegrationEvent;
    }
}