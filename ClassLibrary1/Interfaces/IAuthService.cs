namespace ClassLibrary1.Interfaces
{
    public interface IAuthService
    {
        Task<string> GenerateToken(string authHeader);
    }
}