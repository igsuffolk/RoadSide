using Microsoft.JSInterop;
using System.Text.Json.Serialization;
using System.Text.Json;
using RoadSide.Interfaces;


namespace RoadSide.Services
{
    
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
