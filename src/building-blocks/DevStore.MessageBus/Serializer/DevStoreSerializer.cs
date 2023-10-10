using Confluent.Kafka;
using System.IO;
using System.IO.Compression;
using System.Text.Json;

namespace DevStore.MessageBus.Serializer
{
    public class DevStoreSerializer<T> : ISerializer<T>
    {
        public byte[] Serialize(T data, SerializationContext context)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(data, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            });

            using var memoryStream = new MemoryStream();
            using var zipStream = new GZipStream(memoryStream, CompressionMode.Compress, false);

            zipStream.Write(bytes, 0, bytes.Length);
            zipStream.Close();

            return memoryStream.ToArray();
        }
    }
}
