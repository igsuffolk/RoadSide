using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace RoadSide.Helpers
{
    public class RoadSideAuthenticationStateProvider: AuthenticationStateProvider
    {
        private readonly UserService _userService;

        public RoadSideAuthenticationStateProvider(UserService userService)
        {
            _userService = userService;
        }

        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var principal = new ClaimsPrincipal();
            var user = _userService.FetchUserFromBrowser();

            ///if (user is not null)
            ///{
            ///    var authenticatedUser = await _userService.AuthenticateUserAsync(token);

            ///    if (authenticatedUser is not null)
            ///    {
            ///        principal = authenticatedUser.ToClaimsPrincipal();
            ///    }
            ///}

            return new(principal);
        }

        ///public async Task LoginAsync(string token)
        ///{
        ///    var principal = new ClaimsPrincipal();
        ///    var user = await _userService.AuthenticateUserAsync(token);

        ///    if (user is not null)
        ///    {
        ///        principal = user.ToClaimsPrincipal();
        ///    }

        ///    NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
        ///}

        public void Logout()
        {
           _userService.ClearBrowserUserData();
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new())));
        }
    }
}
