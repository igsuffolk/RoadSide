using Microsoft.JSInterop;
using System.Text.Json.Serialization;
using System.Text.Json;


namespace RoadSide.Services
{
    public interface IStorageService
    {
        Task<string> GetStringAsync(string key);
        Task SetStringAsync(string value, string key);
    }

    public class StorageService(IJSRuntime jsRuntime) : IStorageService
    {
        private readonly IJSRuntime _jsRuntime = jsRuntime;
       
        public async Task<string> GetStringAsync(string key)
        {
            string json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem",key);
            if (string.IsNullOrEmpty(json))
                return default;

            return json;
        }

        public async Task SetStringAsync(string value, string key)
        {
            await _jsRuntime.InvokeAsync<string>("localStorage.setItem", key, value);
        }
    }
}
