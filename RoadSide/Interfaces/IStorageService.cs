namespace RoadSide.Interfaces
{
    public interface IStorageService
    {
        Task<string> GetStringAsync(string key);
        Task SetStringAsync(string value, string key);
    }

}
