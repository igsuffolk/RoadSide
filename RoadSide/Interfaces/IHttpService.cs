namespace RoadSide.Interfaces
{
    public interface IHttpService
    {
        Task<T> GetAsync<T>(string uri);
        Task GetAsyncVoid(string uri);
        Task<T> PostAsync<T>(string uri, T obj);
        Task<bool> PostAsyncBool<T>(string uri, T obj);
        Task<string> PostAsyncString<T>(string uri, T obj);
    }
}
