using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DevStore.Core.Http
{
    public interface IRestClient
    {
        Task<TResult> PostAsync<TRequest, TResult>(TRequest request, string token = null);
    }

    public class RestClient : IRestClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _serializerOptions;

        public RestClient(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
            };
        }

        public async Task<TResult> PostAsync<TRequest, TResult>(TRequest request, string token = null)
        {
            var content = new StringContent(JsonSerializer.Serialize(request, _serializerOptions), Encoding.UTF8, MediaTypeNames.Application.Json);
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = content,
            };

            if (!string.IsNullOrWhiteSpace(token))
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var httpClient = _httpClientFactory.CreateClient(request.GetType().Name);
            var responseMessage = await httpClient.SendAsync(httpRequest);
            var response = await responseMessage.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<TResult>(response, _serializerOptions);
        }
    }
}
