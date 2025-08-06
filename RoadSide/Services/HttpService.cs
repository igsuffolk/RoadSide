
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RoadSide.Services
{
    public interface IHttpService
    {
        Task<T> GetAsync<T>(string uri);
        Task<T> PostAsync<T>(string uri, T obj);
        Task<bool> PostAsyncBool<T>(string uri, T obj);
        Task<string> PostAsyncString<T>(string uri, T obj);
    }
    public class HttpService : IHttpService
    {
        private JsonSerializerOptions jsonOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true

        };

        private readonly IHttpClientFactory _httpClientFactory;

        public HttpService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<T> GetAsync<T>(string uri)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("Api");

            HttpResponseMessage httpResponse = await httpClient.GetAsync(uri);

            if (httpResponse.StatusCode != HttpStatusCode.OK)
            {
                return default(T);
            }

            string result = await httpResponse.Content.ReadAsStringAsync();

            var response = JsonSerializer.Deserialize<T>(result, jsonOptions);

            return response;
        }

        public async Task<T> PostAsync<T>(string uri, T obj)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("Api");

            HttpResponseMessage httpResponse = await httpClient.PostAsJsonAsync<T>(uri, obj);

            if (httpResponse.StatusCode != HttpStatusCode.OK)
            {
                return default(T);
            }

            string result = await httpResponse.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<T>(result, jsonOptions);
        }

        public async Task<bool> PostAsyncBool<T>(string uri, T obj)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("Api");

            HttpResponseMessage httpResponse = await httpClient.PostAsJsonAsync<T>(uri, obj);

            if (httpResponse.StatusCode != HttpStatusCode.OK)
            {
                return false;
            }

            string result = await httpResponse.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<bool>(result, jsonOptions);
        }

        public async Task<string> PostAsyncString<T>(string uri, T obj)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("Api");

            HttpResponseMessage httpResponse = await httpClient.PostAsJsonAsync<T>(uri, obj);

            if (httpResponse.StatusCode != HttpStatusCode.OK)
            {
                return "";
            }

            string result = await httpResponse.Content.ReadAsStringAsync();

            return result;
        }
    }
}
