using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;

namespace RoadSide.Helpers
{
    /// <summary>
    /// AuthenticationStateProvider implementation that reads/writes the JWT token via
    /// the <see cref="UserService"/> and exposes the current user's email as <see cref="Username"/>.
    /// - Builds a ClaimsPrincipal from the stored token when requested by the Blazor auth system.
    /// - Notifies the framework when login/logout occurs so UI can update.
    /// </summary>
    public class RoadSideAuthenticationStateProvider : AuthenticationStateProvider, IDisposable
    {
        private readonly UserService _userService;

        /// <summary>
        /// Publicly accessible username (email) extracted from the current authentication state.
        /// Kept as a convenience for pages/components that want quick access to the current user's name.
        /// </summary>
        public string Username { get; set; } = "";

        public RoadSideAuthenticationStateProvider(UserService userService)
        {
            _userService = userService;

            // Subscribe to authentication state changes so we can update Username when the state changes.
            AuthenticationStateChanged += OnAuthenticationStateChanged;
        }

        /// <summary>
        /// Called by the Blazor authentication system to get the current user's AuthenticationState.
        /// This implementation reads the JWT token from browser storage (via UserService), parses it,
        /// and creates a ClaimsPrincipal populated with token claims. If no valid token exists an empty
        /// (unauthenticated) identity is returned.
        /// </summary>
        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var identity = new ClaimsIdentity();

            // Retrieve token persisted in browser (or empty string if none).
            string token = await _userService.GetToken();

            // If token appears to be a JWT, read claims and create an identity.
            if (tokenHandler.CanReadToken(token))
            {
                var jwtSecurityToken = tokenHandler.ReadJwtToken(token);
                identity = new ClaimsIdentity(jwtSecurityToken.Claims, "RoadSide");
            }

            var principal = new ClaimsPrincipal(identity);
            var authenticationState = new AuthenticationState(principal);

            // Return the authentication state. Keep the asynchronous pattern to match caller expectations.
            var authenticationTask = await Task.FromResult(authenticationState);
            return authenticationTask;
        }

        /// <summary>
        /// Called to persist a successful login token to the browser and notify subscribers
        /// that the authentication state has changed.
        /// </summary>
        /// <param name="token">JWT token string to persist.</param>
        public async Task LoginAsync(string token)
        {
            // Store token in browser storage (local/session) via UserService.
            await _userService.PersistUserToBrowser(token);

            // Notify the Blazor authentication system that the state has changed so it will call
            // GetAuthenticationStateAsync and update any UI depending on authentication.
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        /// <summary>
        /// Clears persisted user data (token) and notifies the auth system that the user is logged out.
        /// Note: async void is used to match the original signature; do not throw from this method.
        /// </summary>
        public async void Logout()
        {
            // Remove token and other user data from browser storage.
            await _userService.ClearBrowserUserData();

            // Trigger an authentication state refresh so UI can react to logout.
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        /// <summary>
        /// Event handler invoked when the AuthenticationState changes. Updates the cached Username
        /// from the ClaimsPrincipal so other components can read it without parsing claims repeatedly.
        /// </summary>
        /// <remarks>
        /// This method is attached to the AuthenticationStateChanged event in the constructor.
        /// </remarks>
        private async void OnAuthenticationStateChanged(Task<AuthenticationState> task)
        {
            var authenticationState = await task;

            if (authenticationState is not null)
            {
                // Try to read the email claim; if not present leave Username empty.
                Username = authenticationState.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? "";
            }
        }

        /// <summary>
        /// Unsubscribe from the AuthenticationStateChanged event to avoid memory leaks.
        /// Call when the provider is disposed.
        /// </summary>
        public void Dispose()
        {
            AuthenticationStateChanged -= OnAuthenticationStateChanged;
        }
    }
}
