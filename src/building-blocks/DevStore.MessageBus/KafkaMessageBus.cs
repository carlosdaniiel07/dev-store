using Confluent.Kafka;
using DevStore.Core.Messages.Integration;
using DevStore.MessageBus.Serializer;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DevStore.MessageBus
{
    public class KafkaMessageBus : IMessageBus
    {
        private readonly string _bootstrapServers;
        private readonly ProducerConfig _producerConfig;
        private readonly JsonSerializerOptions _serializerOptions;

        public KafkaMessageBus(string bootstrapServers)
        {
            _bootstrapServers = bootstrapServers;
            _producerConfig = new ProducerConfig
            {
                BootstrapServers = bootstrapServers,
            };
            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };
        }

        public async Task PublishAsync<T>(T message) where T : IntegrationEvent
        {
            ArgumentNullException.ThrowIfNull(message);
            ArgumentNullException.ThrowIfNull(message.Topic);

            using var producerBuilder = new ProducerBuilder<string, T>(_producerConfig)
                .SetValueSerializer(new DevStoreSerializer<T>())
                .Build();

            await producerBuilder.ProduceAsync(message.Topic, new Message<string, T>
            {
                Key = (message.AggregateId == Guid.Empty ? Guid.NewGuid() : message.AggregateId).ToString(),
                Value = message,
            });
        }

        public async Task SubscribeAsync<T>(string topic, string groupId, Func<T, Task> onMessage, CancellationToken cancellationToken) where T : IntegrationEvent
        {
            await Task.Factory.StartNew(async () =>
            {
                var consumerConfig = new ConsumerConfig
                {
                    GroupId = groupId,
                    BootstrapServers = _bootstrapServers,
                    EnableAutoCommit = false,
                    EnablePartitionEof = true,
                };

                using var consumerBuilder = new ConsumerBuilder<string, T>(consumerConfig)
                    .SetValueDeserializer(new DevStoreDeserializer<T>())
                    .Build();

                consumerBuilder.Subscribe(topic);

                while (!cancellationToken.IsCancellationRequested)
                {
                    var result = consumerBuilder.Consume();

                    if (result.IsPartitionEOF)
                        continue;

                    await onMessage(result.Message.Value);
                    consumerBuilder.Commit(result);
                }
            }, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
    }
}
