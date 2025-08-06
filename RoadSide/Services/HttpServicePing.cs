
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RoadSide.Services
{
    public interface IHttpServicePing
    {
        Task<bool> Ping();
    }
    public class HttpServicePing : IHttpServicePing
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<HttpService> _logger;

        public HttpServicePing(IHttpClientFactory httpClientFactory, ILogger<HttpService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<bool> Ping()
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("Ping");
            try
            {
                HttpResponseMessage httpResponse = await httpClient.GetAsync("Auth/Ping");

                if (httpResponse.StatusCode != HttpStatusCode.OK)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
