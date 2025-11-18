
using RoadSide.Interfaces;
using System.Net;

namespace RoadSide.Services
{
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
