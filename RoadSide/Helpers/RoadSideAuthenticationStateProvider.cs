using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;

namespace RoadSide.Helpers
{
    public class RoadSideAuthenticationStateProvider : AuthenticationStateProvider, IDisposable
    {
        private readonly UserService _userService;
        public string Username { get; set; } = "";

        public RoadSideAuthenticationStateProvider(UserService userService)
        {
            _userService = userService;

            AuthenticationStateChanged += OnAuthenticationStateChanged;
        }

        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var identity = new ClaimsIdentity();
            string token = await _userService.GetToken();

            if (tokenHandler.CanReadToken(token))
            {
                var jwtSecurityToken = tokenHandler.ReadJwtToken(token);
                identity = new ClaimsIdentity(jwtSecurityToken.Claims, "RoadSide");
            }

            var principal = new ClaimsPrincipal(identity);
            var authenticationState = new AuthenticationState(principal);
            var authenticationTask = await Task.FromResult(authenticationState);

            return authenticationTask;
        }

        public async Task LoginAsync(string token)
        {
            await _userService.PersistUserToBrowser(token);

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async void Logout()
        {
            await _userService.ClearBrowserUserData();

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        private async void OnAuthenticationStateChanged(Task<AuthenticationState> task)
        {
            var authenticationState = await task;

            if (authenticationState is not null)
            {
                Username = authenticationState.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? "";
            }
        }
        public void Dispose()
        {
            AuthenticationStateChanged -= OnAuthenticationStateChanged;
        }
    }
}
