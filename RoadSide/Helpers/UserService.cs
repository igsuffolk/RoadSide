using RoadSide.Interfaces;
using RoadSide.Models;
using RoadSide.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RoadSide.Helpers
{
    /// <summary>
    /// Simple abstraction around the browser storage service used to persist
    /// the authentication token (JWT) for the current user.
    /// </summary>
    public class UserService
    {
        private readonly IStorageService _storageService;

        /// <summary>
        /// Key used to store the JWT in browser storage.
        /// Keep this constant in one place to avoid typos when reading/writing.
        /// </summary>
        private const string TokenStorageKey = "Token";

        public UserService(IStorageService storageService)
        {
            _storageService = storageService;
        }

        /// <summary>
        /// Persists the supplied JWT to browser storage so the authentication
        /// state provider and HTTP pipeline can read it later.
        /// </summary>
        /// <param name="token">The JWT string to persist (may be empty to clear).</param>
        public async Task PersistUserToBrowser(string token) =>
            // Note: SetStringAsync takes (value, key) according to IStorageService.
            await _storageService.SetStringAsync(token, TokenStorageKey);

        /// <summary>
        /// Reads the stored JWT from browser storage.
        /// Returns an empty string when no token exists.
        /// </summary>
        public async Task<string> GetToken() =>
            await _storageService.GetStringAsync(TokenStorageKey);

        /// <summary>
        /// Clears persisted user data from the browser. Currently implemented by
        /// setting the stored token value to an empty string.
        /// </summary>
        public async Task ClearBrowserUserData() =>
            await _storageService.SetStringAsync("", TokenStorageKey);
    }
}
