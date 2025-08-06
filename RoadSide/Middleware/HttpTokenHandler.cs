
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.JsonWebTokens;
using RoadSide.Services;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;


namespace RoadSide.Middleware;

public class HttpTokenHandler(IConfiguration configuration, IMemoryCache memoryCache) : DelegatingHandler
{
    private readonly IConfiguration _configuration = configuration;
    private readonly IMemoryCache _memoryCache = memoryCache;


    //	Overrides the SendAsync method from DelegatingHandler.
    //	Retrieves a JWT token using the GetToken method.
    //  Sets the Authorization header of the request with the retrieved token.
    //	Sends the HTTP request to the next handler in the pipeline.
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = new() ;
        try
        {
            // get jwt token from cache or generate new
            var token = await GetToken();

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // unauthorised, clear cache and get new token
                _memoryCache.Remove("ApiToken");
                token = await GetToken();

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                response = await base.SendAsync(request, cancellationToken);
            }
        }
        catch (Exception ex) { }

        return response;

    }

    //	Checks if a token is available in the memory cache.
    //	If not, retrieves a new token using the IHttpService.GetAuth method.
    //	Stores the new token in the memory cache with an expiration time.
    //	Returns the token.
    private async Task<string> GetToken()
    {
        // check cache for token
        if (!_memoryCache.TryGetValue("ApiToken", out string cacheValue))
        {
            string token = await GetJWTToken();

            cacheValue = token;

            var jwtToken = new JsonWebToken(token);
            var tokenExp = jwtToken.Claims.First(claim => claim.Type.Equals("exp")).Value;
            //TimeSpan ts = DdattokenExp - DateTime.Now;

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(DateTimeOffset.FromUnixTimeSeconds(long.Parse(tokenExp)));

            _memoryCache.Set("ApiToken", cacheValue, cacheEntryOptions);
        }

        return cacheValue;


    }
    private async Task<string> GetJWTToken()
    {
        //string clientId = _configuration.GetSection("Jwt").GetValue<string>("ClientId");
        //string clientSecret = _configuration.GetSection("Jwt").GetValue<string>("ClientSecret");

        string clientId = "RoadSide@199#";
        string clientSecret = "p!TgLeW6&!uLQAYbPuonhb";

        HttpClientHandler httpClientHandler = new();
        HttpClient client = new HttpClient(httpClientHandler);

        HttpRequestMessage httpRequest = new()
        {
            RequestUri = new Uri(new(_configuration.GetSection("Jwt").GetValue<string>("Issuer")), "api/auth/token"),
            Method = HttpMethod.Post
        };

        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}")));

        string content = string.Empty;

        HttpResponseMessage response = await client.SendAsync(httpRequest);
        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            content = await response.Content.ReadAsStringAsync();
        }

        client.Dispose();

        return content;

    }

    public class MyToken
    {
        public string access_token { get; set; } = string.Empty;
        public string token_type { get; set; } = string.Empty;
        public int expires_in { get; set; }
        public DateTime expires_at { get; set; }
    }

}
