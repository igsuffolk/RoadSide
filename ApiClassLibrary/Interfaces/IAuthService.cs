namespace ApiClassLibrary.Interfaces
{
    public interface IAuthService
    {
        Task<string?> GenerateToken(string authHeader);
    }
}