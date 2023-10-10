using Confluent.Kafka;
using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json;

namespace DevStore.MessageBus.Serializer
{
    public class DevStoreDeserializer<T> : IDeserializer<T>
    {
        public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            using var memoryStream = new MemoryStream(data.ToArray());
            using var zipStream = new GZipStream(memoryStream, CompressionMode.Decompress, false);

            return JsonSerializer.Deserialize<T>(zipStream, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            });
        }
    }
}
